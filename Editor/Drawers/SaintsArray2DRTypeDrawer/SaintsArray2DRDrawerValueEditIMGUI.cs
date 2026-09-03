#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIEditDrawer;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsArray2DRTypeDrawer
{
    // ReSharper disable once InconsistentNaming
    public partial class SaintsArray2DRDrawer
    {
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

    }
}
#endif
