#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsWrapTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsArray2DRTypeDrawer
{
    // ReSharper disable once InconsistentNaming
    public partial class SaintsArray2DRDrawer
    {
        private const string SerializedRowsName = "_saintsList";
        private const string SerializedColumnsName = "_columnCount";
        private const string SerializedWrapTypeName = "_wrapType";

        private static string NameRoot(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsArray2DR";

        private static string NameListView(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsArray2DR_ListView";

        private static string NameEmptyNotice(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsArray2DR_EmptyNotice";

        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            string label = GetPreferredLabel(property);
            SaintsArray2DRAttribute arrayAttribute =
                saintsAttribute as SaintsArray2DRAttribute ?? new SaintsArray2DRAttribute();
            SaintsArray2DRFoldout root = new SaintsArray2DRFoldout(label)
            {
                viewDataKey = property.propertyPath,
                name = NameRoot(property),
            };
            root.SetTranspose(arrayAttribute.Transpose);

            MultiColumnListView mcl = new MultiColumnListView
            {
                name = NameListView(property),
                viewDataKey = SerializedUtils.GetUniqueId(property),
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                // selectionType = SelectionType.None,
                showBorder = true,
                // showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
            };

            VisualElement emptyNotice = new VisualElement
            {
                name = NameEmptyNotice(property),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            emptyNotice.AddToClassList("unity-collection-view--with-border");
            emptyNotice.Add(new Label("2D Array is Empty")
            {
                style =
                {
                    height = 22,
                    paddingLeft = 6,
                    paddingRight = 2,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    whiteSpace = WhiteSpace.NoWrap,
                },
            });

            root.Add(emptyNotice);
            root.Add(mcl);

            if (!string.IsNullOrEmpty(property.tooltip))
            {
                root.tooltip = property.tooltip;
            }

            return root;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsArray2DRFoldout root = container.Q<SaintsArray2DRFoldout>(NameRoot(property));
            MultiColumnListView listView =
                container.Q<MultiColumnListView>(NameListView(property));
            VisualElement emptyNotice = container.Q<VisualElement>(NameEmptyNotice(property));

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            bool insideArray = arrayIndex != -1;

            UIToolkitUtils.AddContextualMenuManipulator(root.Foldout, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) == -1 ? info.FieldType : ReflectUtils.GetElementType(info.FieldType);
            Debug.Assert(rawType != null, $"Failed to get element type from {property.propertyPath}");
            // Debug.Log(info.FieldType);
            string propNameCompact = GetPropName(rawType);
            SerializedProperty wrapProp = FindPropertyCompact(property, propNameCompact);
            Debug.Assert(wrapProp != null, $"Failed to get prop from {propNameCompact}");
            SerializedProperty columnCountProp = property.FindPropertyRelative(SerializedColumnsName);
            Debug.Assert(columnCountProp != null, $"Failed to get {SerializedColumnsName} from {property.propertyPath}");
            object fieldValue = info.GetValue(parent);
            if (insideArray)
            {
                fieldValue = ((IEnumerable)fieldValue).Cast<object>().ElementAt(arrayIndex);
            }
            (FieldInfo wrapField, object _) = GetTargetInfo(propNameCompact, rawType, fieldValue);
            Debug.Assert(wrapField != null, $"Failed to get field {propNameCompact} from {property.propertyPath}");
            Type rowType = ReflectUtils.GetElementType(wrapField.FieldType);
            Debug.Assert(rowType != null, $"Failed to get row type from {wrapField.FieldType}");
            FieldInfo cellField = GetField(rowType, SerializedRowsName);
            Debug.Assert(cellField != null, $"Failed to get cell field from {rowType}");
            Type cellType = ReflectUtils.GetElementType(cellField.FieldType);
            Debug.Assert(cellType != null, $"Failed to get cell type from {cellField.FieldType}");

            bool hasSerializeReference = false;
            List<InjectAttributeBase> injectAttributes = new List<InjectAttributeBase>();
            List<Attribute> cellAttributes = ReflectCache.GetCustomAttributes<Attribute>(info)
                .Where(each => each is not InjectAttributeBase &&
                               each is not SaintsArrayAttribute &&
                               each is not SaintsArray2DRAttribute)
                .ToList();
            int insideArrayOffset = insideArray ? 1 : 0;
            foreach (InjectAttributeBase injectAttribute in ReflectCache.GetCustomAttributes<InjectAttributeBase>(info))
            {
                if (injectAttribute.Decorator == typeof(SerializeReference))
                {
                    hasSerializeReference = true;
                    continue;
                }

                ValueAttributeAttribute cellInjectAttribute = new ValueAttributeAttribute(
                    injectAttribute.Depth - 3 - insideArrayOffset,
                    injectAttribute.Decorator,
                    injectAttribute.Parameters);
                if (cellInjectAttribute.Depth > 0)
                {
                    injectAttributes.Add(cellInjectAttribute);
                    continue;
                }

                Attribute createdAttribute = SaintsWrapUtils.CreateInjectedAttribute(cellInjectAttribute);
                if (createdAttribute != null)
                {
                    cellAttributes.Add(createdAttribute);
                }
            }

            WrapType cellWrapType = SaintsWrapUtils.EnsureWrapType(
                property.FindPropertyRelative(SerializedWrapTypeName), cellField, hasSerializeReference);

            root.ColSizeField.SetValueWithoutNotify(wrapProp.arraySize == 0
                ? Mathf.Max(0, columnCountProp.intValue)
                : Mathf.Max(0, GetColumnCount(wrapProp)));

            root.ColReduceButton.clicked += () => SetColumnCount(root.ColSizeField.value - 1);
            root.ColAddButton.clicked += () => SetColumnCount(root.ColSizeField.value + 1);
            root.RowReduceButton.clicked += () => SetRowCount(wrapProp.arraySize - 1);
            root.RowAddButton.clicked += () => SetRowCount(wrapProp.arraySize + 1);
            root.ColSizeField.RegisterValueChangedCallback(evt => SetColumnCount(evt.newValue));
            root.RowSizeField.RegisterValueChangedCallback(evt => SetRowCount(evt.newValue));
            root.TrackPropertyValue(wrapProp, _ => Refresh());
            root.TrackPropertyValue(columnCountProp, trackedProperty =>
            {
                if (wrapProp.arraySize == 0)
                {
                    root.ColSizeField.SetValueWithoutNotify(Mathf.Max(0, trackedProperty.intValue));
                }
                Refresh();
            });
            listView.RegisterCallback<GeometryChangedEvent>(_ => SyncColumnWidths(listView));
            listView.itemIndexChanged += (fromIndex, toIndex) =>
            {
                wrapProp.MoveArrayElement(fromIndex, toIndex);
                property.serializedObject.ApplyModifiedProperties();
                Util.PropertyChangedCallback(property, info, onValueChangedCallback);
                Refresh();
            };
            bool resettingColumnDisplayOrder = false;
            RegisterColumnReorderedCallback(listView.columns, (_, fromIndex, toIndex) =>
            {
                int columnCount = wrapProp.arraySize == 0 ? 0 : GetColumnCount(wrapProp);
                if (resettingColumnDisplayOrder || fromIndex == toIndex || fromIndex < 0 || toIndex < 0 ||
                    fromIndex >= columnCount || toIndex >= columnCount)
                {
                    return;
                }

                for (int rowIndex = 0; rowIndex < wrapProp.arraySize; rowIndex++)
                {
                    SerializedProperty row = wrapProp.GetArrayElementAtIndex(rowIndex);
                    row.FindPropertyRelative(SerializedRowsName).MoveArrayElement(fromIndex, toIndex);
                }

                property.serializedObject.ApplyModifiedProperties();
                Util.PropertyChangedCallback(property, info, onValueChangedCallback);
                listView.schedule.Execute(() =>
                {
                    resettingColumnDisplayOrder = true;
                    try
                    {
                        listView.columns.ReorderDisplay(toIndex, fromIndex);
                    }
                    finally
                    {
                        resettingColumnDisplayOrder = false;
                    }
                    Refresh();
                });
            });

            Refresh();
            return;

            void SetColumnCount(int value)
            {
                int newColumnCount = Mathf.Max(0, value);
                bool changed = columnCountProp.intValue != newColumnCount;
                columnCountProp.intValue = newColumnCount;
                for (int rowIndex = 0; rowIndex < wrapProp.arraySize; rowIndex++)
                {
                    SerializedProperty row = wrapProp.GetArrayElementAtIndex(rowIndex);
                    SerializedProperty columns = row.FindPropertyRelative(SerializedRowsName);
                    if (columns.arraySize != newColumnCount)
                    {
                        columns.arraySize = newColumnCount;
                        changed = true;
                    }
                }

                if (changed)
                {
                    property.serializedObject.ApplyModifiedProperties();
                    Util.PropertyChangedCallback(property, info, onValueChangedCallback);
                }
                Refresh();
            }

            void SetRowCount(int value)
            {
                int newRowCount = Mathf.Max(0, value);
                int columnCount = Mathf.Max(0, root.ColSizeField.value);
                bool changed = wrapProp.arraySize != newRowCount;
                changed |= columnCountProp.intValue != columnCount;
                columnCountProp.intValue = columnCount;

                wrapProp.arraySize = newRowCount;
                for (int rowIndex = 0; rowIndex < newRowCount; rowIndex++)
                {
                    SerializedProperty row = wrapProp.GetArrayElementAtIndex(rowIndex);
                    SerializedProperty columns = row.FindPropertyRelative(SerializedRowsName);
                    if (columns.arraySize != columnCount)
                    {
                        columns.arraySize = columnCount;
                        changed = true;
                    }
                }

                if (changed)
                {
                    property.serializedObject.ApplyModifiedProperties();
                    Util.PropertyChangedCallback(property, info, onValueChangedCallback);
                }
                Refresh();
            }

            void Refresh()
            {
                // 行
                int rowCount = wrapProp.arraySize;
                root.RowSizeField.SetValueWithoutNotify(rowCount);
                root.RowReduceButton.SetEnabled(rowCount > 0);
                if (rowCount == 0)
                {
                    int emptyColumnCount = Mathf.Max(0, root.ColSizeField.value);
                    root.ColSizeField.SetValueWithoutNotify(emptyColumnCount);
                    root.ColSizeField.SetEnabled(true);
                    root.ColReduceButton.SetEnabled(emptyColumnCount > 0);
                    root.ColAddButton.SetEnabled(true);
                    emptyNotice.style.display = DisplayStyle.Flex;
                    listView.style.display = DisplayStyle.None;
                    listView.itemsSource = Array.Empty<object>();
                    listView.Rebuild();
                    return;
                }

                emptyNotice.style.display = DisplayStyle.None;
                listView.style.display = DisplayStyle.Flex;

                // 列
                SerializedProperty saintList0 = wrapProp.GetArrayElementAtIndex(0);
                SerializedProperty itemActualListProp = saintList0.FindPropertyRelative(SerializedRowsName);
                int columnCount = itemActualListProp.arraySize;
                if (columnCountProp.intValue != columnCount)
                {
                    columnCountProp.intValue = columnCount;
                    property.serializedObject.ApplyModifiedProperties();
                }
                root.ColSizeField.SetValueWithoutNotify(columnCount);
                root.ColSizeField.SetEnabled(true);
                root.ColReduceButton.SetEnabled(columnCount > 0);
                root.ColAddButton.SetEnabled(true);

                if (columnCount == 0)
                {
                    emptyNotice.style.display = DisplayStyle.Flex;
                    listView.style.display = DisplayStyle.None;
                    listView.itemsSource = Array.Empty<object>();
                    listView.Rebuild();
                    return;
                }

                ReconcileColumns(listView, columnCount, wrapProp, cellWrapType, cellField, cellType,
                    cellAttributes, injectAttributes, hasSerializeReference, wrapField);
                listView.itemsSource = Enumerable.Range(0, rowCount).ToList();
                listView.Rebuild();
                listView.schedule.Execute(() => SyncColumnWidths(listView));
            }
        }

        private static int GetColumnCount(SerializedProperty wrapProp)
        {
            SerializedProperty firstRow = wrapProp.GetArrayElementAtIndex(0);
            return firstRow.FindPropertyRelative(SerializedRowsName).arraySize;
        }

        private static void RegisterColumnReorderedCallback(Columns columns,
            Action<Column, int, int> callback)
        {
            EventInfo columnReorderedEvent = typeof(Columns).GetEvent("columnReordered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            columnReorderedEvent?.GetAddMethod(true)?.Invoke(columns, new object[] { callback });
        }

        private void ReconcileColumns(MultiColumnListView listView,
            int columnCount, SerializedProperty wrapProp, WrapType cellWrapType, FieldInfo cellField,
            Type cellType, IReadOnlyList<Attribute> cellAttributes,
            IReadOnlyList<InjectAttributeBase> injectAttributes, bool hasSerializeReference,
            FieldInfo wrapField)
        {
            for (int columnIndex = listView.columns.Count; columnIndex < columnCount; columnIndex++)
            {
                int capturedColumn = columnIndex;
                listView.columns.Add(new Column
                {
                    name = $"column-{columnIndex}",
                    title = $"{columnIndex}",
                    // minWidth = 120,
                    stretchable = false,
                    makeCell = () => new VisualElement
                    {
                        style =
                        {
                            flexGrow = 1,
                            justifyContent = Justify.Center,
                        },
                    },
                    bindCell = (element, rowIndex) =>
                    {
                        BindCell(element, wrapProp, rowIndex, capturedColumn, cellWrapType, cellField,
                            cellType, cellAttributes, injectAttributes, hasSerializeReference, wrapField);
                    },
                    unbindCell = (element, _) =>
                    {
                        element.Unbind();
                        element.Clear();
                    },
                });
            }

            while (listView.columns.Count > columnCount)
            {
                listView.columns.RemoveAt(listView.columns.Count - 1);
            }

            listView.columns.primaryColumnName = "column-0";
        }

        private static void SyncColumnWidths(MultiColumnListView listView)
        {
            int columnCount = listView.columns.Count;
            if (columnCount == 0 || listView.resolvedStyle.display == DisplayStyle.None)
            {
                return;
            }

            ScrollView scrollView = listView.Q<ScrollView>();
            float availableWidth = scrollView?.contentViewport.resolvedStyle.width ?? listView.contentRect.width;
            if (float.IsNaN(availableWidth) || availableWidth <= 0)
            {
                return;
            }

            const float dragHandleWidth = 15f;
            const float reorderableContainerPadding = 18f;
            const float firstColumnOverhead = dragHandleWidth + reorderableContainerPadding;
            float cellWidth = Mathf.Max(0, (availableWidth - firstColumnOverhead) / columnCount);
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                float width = cellWidth + (columnIndex == 0 ? firstColumnOverhead : 0);
                Column column = listView.columns[columnIndex];
                if (!Mathf.Approximately(column.width.value, width))
                {
                    column.width = width;
                }
            }
        }

        private void BindCell(VisualElement element, SerializedProperty wrapProp, int rowIndex, int columnIndex,
            WrapType cellWrapType, FieldInfo cellField, Type cellType,
            IReadOnlyList<Attribute> cellAttributes, IReadOnlyList<InjectAttributeBase> injectAttributes,
            bool hasSerializeReference, FieldInfo wrapField)
        {
            element.Unbind();
            element.Clear();

            if (rowIndex < 0 || rowIndex >= wrapProp.arraySize)
            {
                return;
            }

            SerializedProperty saintListProp = wrapProp.GetArrayElementAtIndex(rowIndex);
            SerializedProperty itemActualListWrapProp = saintListProp.FindPropertyRelative(SerializedRowsName);

            if (columnIndex < 0 || columnIndex >= itemActualListWrapProp.arraySize)
            {
                element.Add(new Label("-"));
                return;
            }

            SerializedProperty saintsWrapProp = itemActualListWrapProp.GetArrayElementAtIndex(columnIndex);
            saintsWrapProp.isExpanded = true;

            object wrapParent = SerializedUtils.GetFieldInfoAndDirectParent(wrapProp).parent;
            IList rows = wrapParent == null ? null : wrapField.GetValue(wrapParent) as IList;
            if (rows == null || rowIndex >= rows.Count)
            {
                element.Add(new Label("-"));
                return;
            }
            object rowParent = rows[rowIndex];

            VisualElement resultElement = SaintsWrapUtils.CreateCellElement(
                cellWrapType,
                cellField,
                cellType,
                saintsWrapProp,
                cellAttributes,
                injectAttributes,
                hasSerializeReference,
                this,
                this,
                this,
                rowParent);
            element.Add(resultElement);
        }

        private static SerializedProperty GetWrappedValueProperty(SerializedProperty cell)
        {
            SerializedProperty wrapTypeProperty = cell.FindPropertyRelative("wrapType");
            WrapType wrapType = wrapTypeProperty == null
                ? WrapType.Undefined
                : (WrapType)wrapTypeProperty.intValue;
            string propertyName;
            switch (wrapType)
            {
                case WrapType.Field:
                    propertyName = "valueField";
                    break;
                case WrapType.Array:
                    propertyName = "valueArray";
                    break;
                case WrapType.List:
                    propertyName = "valueList";
                    break;
                default:
                    propertyName = "value";
                    break;
            }

            return cell.FindPropertyRelative(propertyName) ?? cell;
        }

        private static FieldInfo GetField(Type type, string fieldName)
        {
            if (type == null)
            {
                return null;
            }

            return type.GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                BindingFlags.FlattenHierarchy);
        }
    }
}
#endif
