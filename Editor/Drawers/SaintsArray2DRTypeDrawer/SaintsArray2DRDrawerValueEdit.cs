#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIEditDrawer;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsArray2DRTypeDrawer
{
    // ReSharper disable once InconsistentNaming
    public partial class SaintsArray2DRDrawer
    {
        private sealed class Array2DRValueElement : VisualElement
        {
            private readonly SaintsArray2DRFoldout _root;
            private readonly MultiColumnListView _listView;
            private readonly VisualElement _emptyNotice;
            private readonly string _viewKey;
            private bool _resettingColumnDisplayOrder;

            private Type _valueType;
            private Array _array;
            private Action<object> _beforeSet;
            private Action<object> _setter;
            private bool _labelGrayColor;
            private bool _inHorizontalLayout;
            private IReadOnlyList<Attribute> _cellAttributes;
            private IReadOnlyList<object> _targets;
            private IRichTextTagProvider _richTextTagProvider;

            public Array2DRValueElement(string label, string viewKey)
            {
                _viewKey = viewKey;
                _root = new SaintsArray2DRFoldout(label)
                {
                    viewDataKey = viewKey,
                };
                Add(_root);

                _listView = new MultiColumnListView
                {
                    viewDataKey = viewKey,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    showBorder = true,
                    reorderable = true,
                    reorderMode = ListViewReorderMode.Animated,
                };
                _emptyNotice = new VisualElement
                {
                    style =
                    {
                        display = DisplayStyle.None,
                        minHeight = 22,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                        borderTopWidth = 1,
                        borderBottomWidth = 1,
                        borderLeftColor = new Color(0.1254902f, 0.1254902f, 0.1254902f),
                        borderRightColor = new Color(0.1254902f, 0.1254902f, 0.1254902f),
                        borderTopColor = new Color(0.1254902f, 0.1254902f, 0.1254902f),
                        borderBottomColor = new Color(0.1254902f, 0.1254902f, 0.1254902f),
                    },
                };
                _emptyNotice.Add(new Label("2D Array is Empty")
                {
                    style =
                    {
                        height = 22,
                        paddingLeft = 6,
                        unityTextAlign = TextAnchor.MiddleLeft,
                    },
                });
                _root.Add(_emptyNotice);
                _root.Add(_listView);

                _root.ColReduceButton.clicked += () => Resize(GetRowCount(), GetColumnCount() - 1);
                _root.ColAddButton.clicked += () => Resize(GetRowCount(), GetColumnCount() + 1);
                _root.RowReduceButton.clicked += () => Resize(GetRowCount() - 1, GetColumnCount());
                _root.RowAddButton.clicked += () => Resize(GetRowCount() + 1, GetColumnCount());
                _root.ColSizeField.RegisterValueChangedCallback(evt => Resize(GetRowCount(), evt.newValue));
                _root.RowSizeField.RegisterValueChangedCallback(evt => Resize(evt.newValue, GetColumnCount()));
                _root.RegisterValueChangedCallback(evt => SessionState.SetBool(_viewKey, evt.newValue));
                _listView.RegisterCallback<GeometryChangedEvent>(_ => SyncColumnWidths(_listView));
                _listView.itemIndexChanged += (fromIndex, toIndex) =>
                {
                    int rowCount = GetRowCount();
                    if (_setter == null || fromIndex == toIndex || fromIndex < 0 || toIndex < 0 ||
                        fromIndex >= rowCount || toIndex >= rowCount)
                    {
                        return;
                    }

                    Commit(MoveDimension(_array, 0, fromIndex, toIndex));
                };
                RegisterColumnReorderedCallback(_listView.columns, (_, fromIndex, toIndex) =>
                {
                    int columnCount = GetColumnCount();
                    if (_resettingColumnDisplayOrder || _setter == null || fromIndex == toIndex ||
                        fromIndex < 0 || toIndex < 0 || fromIndex >= columnCount || toIndex >= columnCount)
                    {
                        return;
                    }

                    Commit(MoveDimension(_array, 1, fromIndex, toIndex));
                    _listView.schedule.Execute(() =>
                    {
                        _resettingColumnDisplayOrder = true;
                        try
                        {
                            _listView.columns.ReorderDisplay(toIndex, fromIndex);
                        }
                        finally
                        {
                            _resettingColumnDisplayOrder = false;
                        }
                        Refresh();
                    });
                });

                _root.SetValueWithoutNotify(SessionState.GetBool(_viewKey, false));
            }

            public void UpdateValue(string label, Type valueType, Array array,
                Action<object> beforeSet, Action<object> setter, bool labelGrayColor,
                bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes,
                IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider)
            {
                _root.Foldout.text = label;
                _valueType = valueType ?? array?.GetType();
                _array = array;
                _beforeSet = beforeSet;
                _setter = setter;
                _labelGrayColor = labelGrayColor;
                _inHorizontalLayout = inHorizontalLayout;
                _cellAttributes = allAttributes
                    .Where(each => each is not SaintsArrayAttribute && each is not SaintsArray2DRAttribute)
                    .ToArray();
                _targets = targets;
                _richTextTagProvider = richTextTagProvider;
                _root.SetTranspose(allAttributes.OfType<SaintsArray2DRAttribute>().FirstOrDefault()?.Transpose == true);
                Label labelElement = _root.Foldout.Q<Label>();
                if (labelElement != null)
                {
                    labelElement.style.color = _labelGrayColor
                        ? EColor.EditorSeparator.GetColor()
                        : new StyleColor(StyleKeyword.Null);
                }
                Refresh();
            }

            private void Refresh()
            {
                int rowCount = GetRowCount();
                int columnCount = GetColumnCount();
                bool editable = _setter != null;
                _root.RowSizeField.SetValueWithoutNotify(rowCount);
                _root.ColSizeField.SetValueWithoutNotify(columnCount);
                _root.RowSizeField.SetEnabled(editable);
                _root.ColSizeField.SetEnabled(editable);
                _root.RowReduceButton.SetEnabled(editable && rowCount > 0);
                _root.ColReduceButton.SetEnabled(editable && columnCount > 0);
                _root.RowAddButton.SetEnabled(editable);
                _root.ColAddButton.SetEnabled(editable);
                _listView.reorderable = editable;

                if (rowCount == 0 || columnCount == 0)
                {
                    _emptyNotice.style.display = DisplayStyle.Flex;
                    _listView.style.display = DisplayStyle.None;
                    _listView.itemsSource = Array.Empty<object>();
                    _listView.Rebuild();
                    return;
                }

                _emptyNotice.style.display = DisplayStyle.None;
                _listView.style.display = DisplayStyle.Flex;
                ReconcileColumns(columnCount);
                _listView.itemsSource = Enumerable.Range(0, rowCount).ToList();
                _listView.Rebuild();
                _listView.schedule.Execute(() => SyncColumnWidths(_listView));
            }

            private void ReconcileColumns(int columnCount)
            {
                for (int columnIndex = _listView.columns.Count; columnIndex < columnCount; columnIndex++)
                {
                    int capturedColumn = columnIndex;
                    _listView.columns.Add(new Column
                    {
                        name = $"column-{columnIndex}",
                        title = columnIndex.ToString(),
                        stretchable = false,
                        makeCell = () => new VisualElement
                        {
                            style =
                            {
                                flexGrow = 1,
                                justifyContent = Justify.Center,
                            },
                        },
                        bindCell = (element, rowIndex) => BindCell(element, rowIndex, capturedColumn),
                        unbindCell = (element, _) => element.Clear(),
                    });
                }

                while (_listView.columns.Count > columnCount)
                {
                    _listView.columns.RemoveAt(_listView.columns.Count - 1);
                }
                _listView.columns.primaryColumnName = "column-0";
            }

            private void BindCell(VisualElement element, int rowIndex, int columnIndex)
            {
                element.Clear();
                if (_array == null || rowIndex >= GetRowCount() || columnIndex >= GetColumnCount())
                {
                    return;
                }

                object cellValue = _array.GetValue(rowIndex, columnIndex);
                Action<object> cellSetter = _setter == null
                    ? null
                    : newValue =>
                    {
                        Array newArray = (Array)_array.Clone();
                        newArray.SetValue(newValue, rowIndex, columnIndex);
                        Commit(newArray);
                    };
                (VisualElement result, bool _) = UIToolkitEdit.UIToolkitValueEdit(
                    null, null, GetElementType(), cellValue, null, cellSetter,
                    _labelGrayColor, _inHorizontalLayout, _cellAttributes, _targets,
                    _richTextTagProvider, $"{_viewKey}[{rowIndex},{columnIndex}]");
                if (result != null)
                {
                    element.Add(result);
                }
            }

            private void Resize(int rowCount, int columnCount)
            {
                if (_setter == null)
                {
                    return;
                }

                int newRowCount = Mathf.Max(0, rowCount);
                int newColumnCount = Mathf.Max(0, columnCount);
                if (newRowCount == GetRowCount() && newColumnCount == GetColumnCount())
                {
                    return;
                }

                Array resized = Array.CreateInstance(GetElementType(), newRowCount, newColumnCount);
                if (_array != null)
                {
                    int copyRows = Mathf.Min(newRowCount, GetRowCount());
                    int copyColumns = Mathf.Min(newColumnCount, GetColumnCount());
                    for (int row = 0; row < copyRows; row++)
                    {
                        for (int column = 0; column < copyColumns; column++)
                        {
                            resized.SetValue(_array.GetValue(row, column), row, column);
                        }
                    }
                }
                Commit(resized);
            }

            private void Commit(Array value)
            {
                _beforeSet?.Invoke(_array);
                _setter?.Invoke(value);
                _array = value;
                Refresh();
            }

            private int GetRowCount() => _array?.GetLength(0) ?? 0;
            private int GetColumnCount() => _array?.GetLength(1) ?? 0;
            private Type GetElementType() => _valueType?.GetElementType() ?? _array?.GetType().GetElementType() ?? typeof(object);
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label,
            Type valueType, Array value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            string viewKey = $"{foldoutViewKey}.array2dr";
            if (oldElement is Array2DRValueElement old && old.ClassListContains(viewKey))
            {
                old.UpdateValue(label, valueType, value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, allAttributes, targets, richTextTagProvider);
                return null;
            }

            Array2DRValueElement element = new Array2DRValueElement(label, viewKey);
            element.AddToClassList(viewKey);
            element.UpdateValue(label, valueType, value, beforeSet, setterOrNull, labelGrayColor,
                inHorizontalLayout, allAttributes, targets, richTextTagProvider);
            return element;
        }

        public static float GetIMGUIValueEditHeight(string label, Type valueType, Array value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor,
            bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            string key = EnsureIMGUIValueEditKey(foldoutViewKey);
            if (!IMGUIEdit.ViewKey[key])
            {
                return SaintsPropertyDrawer.SingleLineHeight;
            }

            int rowCount = value?.GetLength(0) ?? 0;
            int columnCount = value?.GetLength(1) ?? 0;
            float height = SaintsPropertyDrawer.SingleLineHeight + ContentGap;
            if (rowCount == 0 || columnCount == 0)
            {
                return height + SaintsPropertyDrawer.SingleLineHeight;
            }

            Type elementType = valueType?.GetElementType() ?? value.GetType().GetElementType();
            IReadOnlyList<Attribute> cellAttributes = GetValueEditCellAttributes(allAttributes);
            height += SaintsPropertyDrawer.SingleLineHeight + CellGap;
            for (int row = 0; row < rowCount; row++)
            {
                float rowHeight = SaintsPropertyDrawer.SingleLineHeight;
                for (int column = 0; column < columnCount; column++)
                {
                    rowHeight = Mathf.Max(rowHeight, IMGUIEdit.GetPropertyHeight("", elementType,
                        value.GetValue(row, column), null, setterOrNull == null ? null : _ => { },
                        labelGrayColor, inHorizontalLayout, cellAttributes, targets,
                        richTextTagProvider, $"{key}[{row},{column}]"));
                }
                height += rowHeight + CellGap;
            }
            return height;
        }

        public static void IMGUIValueEditOnGUI(Rect position, string label, Type valueType, Array value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor,
            bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            string key = EnsureIMGUIValueEditKey(foldoutViewKey);
            int rowCount = value?.GetLength(0) ?? 0;
            int columnCount = value?.GetLength(1) ?? 0;
            Type elementType = valueType?.GetElementType() ?? value?.GetType().GetElementType() ?? typeof(object);
            SaintsArray2DRAttribute arrayAttribute = allAttributes.OfType<SaintsArray2DRAttribute>().FirstOrDefault();

            Rect headerRect = new Rect(position)
            {
                height = SaintsPropertyDrawer.SingleLineHeight,
            };
            float controlsWidth = HeaderControlWidth * 2 + HeaderControlGap;
            Rect foldoutRect = new Rect(headerRect)
            {
                width = Mathf.Max(0f, headerRect.width - controlsWidth - HeaderControlGap),
            };
            using (new LabelColorScoop(labelGrayColor))
            {
                IMGUIEdit.ViewKey[key] = EditorGUI.Foldout(foldoutRect, IMGUIEdit.ViewKey[key], label, true);
            }

            Rect firstControl = new Rect(headerRect.xMax - controlsWidth, headerRect.y,
                HeaderControlWidth, headerRect.height);
            Rect secondControl = new Rect(firstControl.xMax + HeaderControlGap, headerRect.y,
                HeaderControlWidth, headerRect.height);
            using (new EditorGUI.DisabledScope(setterOrNull == null))
            {
                if (arrayAttribute?.Transpose == true)
                {
                    DrawDimensionControlIMGUI(firstControl, "H", rowCount,
                        newRows => CommitResize(value, elementType, newRows, columnCount, beforeSet, setterOrNull));
                    DrawDimensionControlIMGUI(secondControl, "W", columnCount,
                        newColumns => CommitResize(value, elementType, rowCount, newColumns, beforeSet, setterOrNull));
                }
                else
                {
                    DrawDimensionControlIMGUI(firstControl, "W", columnCount,
                        newColumns => CommitResize(value, elementType, rowCount, newColumns, beforeSet, setterOrNull));
                    DrawDimensionControlIMGUI(secondControl, "H", rowCount,
                        newRows => CommitResize(value, elementType, newRows, columnCount, beforeSet, setterOrNull));
                }
            }

            if (!IMGUIEdit.ViewKey[key])
            {
                return;
            }

            Rect contentRect = new Rect(position.x, headerRect.yMax + ContentGap, position.width,
                Mathf.Max(0f, position.yMax - headerRect.yMax - ContentGap));
            if (rowCount == 0 || columnCount == 0)
            {
                EditorGUI.HelpBox(new Rect(contentRect)
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                }, "2D Array is Empty", MessageType.None);
                return;
            }

            float cellWidth = GetCellWidth(contentRect.width, columnCount);
            DrawColumnLabelsIMGUI(new Rect(contentRect)
            {
                height = SaintsPropertyDrawer.SingleLineHeight,
            }, columnCount, cellWidth);
            IReadOnlyList<Attribute> cellAttributes = GetValueEditCellAttributes(allAttributes);
            float y = contentRect.y + SaintsPropertyDrawer.SingleLineHeight + CellGap;
            for (int row = 0; row < rowCount; row++)
            {
                float rowHeight = SaintsPropertyDrawer.SingleLineHeight;
                for (int column = 0; column < columnCount; column++)
                {
                    rowHeight = Mathf.Max(rowHeight, IMGUIEdit.GetPropertyHeight("", elementType,
                        value.GetValue(row, column), null, setterOrNull == null ? null : _ => { },
                        labelGrayColor, inHorizontalLayout, cellAttributes, targets,
                        richTextTagProvider, $"{key}[{row},{column}]"));
                }

                Rect rowRect = new Rect(contentRect.x, y, contentRect.width, rowHeight);
                EditorGUI.LabelField(new Rect(rowRect.x, rowRect.y, RowLabelWidth, rowRect.height),
                    row.ToString(), EditorStyles.miniLabel);
                for (int column = 0; column < columnCount; column++)
                {
                    int capturedRow = row;
                    int capturedColumn = column;
                    Rect cellRect = GetCellRect(rowRect, column, cellWidth);
                    IMGUIEdit.OnGUI(cellRect, "", elementType, value.GetValue(row, column), null,
                        setterOrNull == null
                            ? null
                            : newCellValue =>
                            {
                                Array changedArray = (Array)value.Clone();
                                changedArray.SetValue(newCellValue, capturedRow, capturedColumn);
                                beforeSet?.Invoke(value);
                                setterOrNull(changedArray);
                            },
                        labelGrayColor, inHorizontalLayout, cellAttributes, targets,
                        richTextTagProvider, $"{key}[{row},{column}]");
                }
                y += rowHeight + CellGap;
            }
        }

        private static string EnsureIMGUIValueEditKey(string foldoutViewKey)
        {
            string key = $"{foldoutViewKey}.array2dr";
            if (!IMGUIEdit.ViewKey.ContainsKey(key))
            {
                IMGUIEdit.ViewKey[key] = true;
            }
            return key;
        }

        private static IReadOnlyList<Attribute> GetValueEditCellAttributes(IReadOnlyList<Attribute> allAttributes)
        {
            return allAttributes.Where(each => each is not SaintsArrayAttribute &&
                                               each is not SaintsArray2DRAttribute).ToArray();
        }

        private static void CommitResize(Array source, Type elementType, int rowCount, int columnCount,
            Action<object> beforeSet, Action<object> setter)
        {
            if (setter == null)
            {
                return;
            }

            int newRowCount = Mathf.Max(0, rowCount);
            int newColumnCount = Mathf.Max(0, columnCount);
            Array resized = Array.CreateInstance(elementType, newRowCount, newColumnCount);
            if (source != null)
            {
                int copyRows = Mathf.Min(source.GetLength(0), newRowCount);
                int copyColumns = Mathf.Min(source.GetLength(1), newColumnCount);
                for (int row = 0; row < copyRows; row++)
                {
                    for (int column = 0; column < copyColumns; column++)
                    {
                        resized.SetValue(source.GetValue(row, column), row, column);
                    }
                }
            }
            beforeSet?.Invoke(source);
            setter.Invoke(resized);
        }

        private static Array MoveDimension(Array source, int dimension, int fromIndex, int toIndex)
        {
            if (source == null || fromIndex == toIndex)
            {
                return source;
            }

            int rows = source.GetLength(0);
            int columns = source.GetLength(1);
            Array result = Array.CreateInstance(source.GetType().GetElementType(), rows, columns);
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int targetIndex = dimension == 0 ? row : column;
                    int sourceIndex = GetMovedSourceIndex(targetIndex, fromIndex, toIndex);
                    int sourceRow = dimension == 0 ? sourceIndex : row;
                    int sourceColumn = dimension == 1 ? sourceIndex : column;
                    result.SetValue(source.GetValue(sourceRow, sourceColumn), row, column);
                }
            }
            return result;
        }

        private static int GetMovedSourceIndex(int targetIndex, int fromIndex, int toIndex)
        {
            if (targetIndex == toIndex)
            {
                return fromIndex;
            }
            if (fromIndex < toIndex && targetIndex >= fromIndex && targetIndex < toIndex)
            {
                return targetIndex + 1;
            }
            if (fromIndex > toIndex && targetIndex > toIndex && targetIndex <= fromIndex)
            {
                return targetIndex - 1;
            }
            return targetIndex;
        }
    }
}
#endif
