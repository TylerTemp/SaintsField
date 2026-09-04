using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsWrapTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsArray2DRTypeDrawer
{
    // ReSharper disable once InconsistentNaming
    public partial class SaintsArray2DRDrawer
    {
        private sealed class ElementContextIMGUI
        {
            public SerializedProperty RootProperty;
            public SerializedProperty RowsProperty;
            public FieldInfo CellField;
            public Type CellType;
            public WrapType CellWrapType;
            public bool HasSerializeReference;
            public IReadOnlyList<Attribute> CellAttributes;
            public FieldInfo Info;
            public object Parent;
            public GUIContent Label;
            public bool InHorizontalLayout;
        }

        private sealed class InfoIMGUI
        {
            public ElementContextIMGUI Context;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI =
            new Dictionary<string, InfoIMGUI>();

        private const float HeaderControlWidth = 88f;
        private const float HeaderControlGap = 4f;
        private const float HeaderDimensionLabelWidth = 14f;
        private const float HeaderButtonWidth = 18f;
        private const float RowLabelWidth = 28f;
        private const float CellGap = 2f;
        private const float ContentGap = 2f;

        protected override bool UseCreateFieldIMGUI => true;

        private static InfoIMGUI EnsureKeyIMGUI(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cached))
            {
                return cached;
            }

            InfoCacheIMGUI[key] = cached = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key,
                () => InfoCacheIMGUI.Remove(key));
            return cached;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width, int index, ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth,
            object parent)
        {
            if (!TryBuildElementContextIMGUI(property, label, info, parent, out ElementContextIMGUI context))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            EnsureKeyIMGUI(property).Context = context;
            if (!property.isExpanded)
            {
                return SaintsPropertyDrawer.SingleLineHeight;
            }

            int rowCount = context.RowsProperty.arraySize;
            int columnCount = GetColumnCountIMGUI(context);
            float height = SaintsPropertyDrawer.SingleLineHeight + ContentGap;
            if (rowCount == 0 || columnCount == 0)
            {
                return height + SaintsPropertyDrawer.SingleLineHeight;
            }

            float cellWidth = GetCellWidth(width, columnCount);
            height += SaintsPropertyDrawer.SingleLineHeight + CellGap;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                height += GetRowHeightIMGUI(context, rowIndex, columnCount, cellWidth) + CellGap;
            }

            return height;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            if (!TryBuildElementContextIMGUI(property, label, info, parent, out ElementContextIMGUI context))
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            EnsureKeyIMGUI(property).Context = context;
            SaintsArray2DRAttribute arrayAttribute =
                saintsAttribute as SaintsArray2DRAttribute ?? new SaintsArray2DRAttribute();

            Rect headerRect = new Rect(position)
            {
                height = SaintsPropertyDrawer.SingleLineHeight,
            };
            DrawHeaderIMGUI(headerRect, context, arrayAttribute);
            if (!property.isExpanded)
            {
                return;
            }

            int rowCount = context.RowsProperty.arraySize;
            int columnCount = GetColumnCountIMGUI(context);
            Rect contentRect = new Rect(position)
            {
                y = headerRect.yMax + ContentGap,
                height = Mathf.Max(0f, position.yMax - headerRect.yMax - ContentGap),
            };
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

            float y = contentRect.y + SaintsPropertyDrawer.SingleLineHeight + CellGap;
            bool changed = false;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                float rowHeight = GetRowHeightIMGUI(context, rowIndex, columnCount, cellWidth);
                Rect rowRect = new Rect(contentRect.x, y, contentRect.width, rowHeight);
                changed |= DrawRowIMGUI(rowRect, context, rowIndex, columnCount, cellWidth);
                y += rowHeight + CellGap;
            }

            if (changed)
            {
                ApplyAndTriggerIMGUI(context);
            }
        }

        private void DrawHeaderIMGUI(Rect rect, ElementContextIMGUI context,
            SaintsArray2DRAttribute arrayAttribute)
        {
            int rowCount = context.RowsProperty.arraySize;
            int columnCount = GetColumnCountIMGUI(context);
            float controlsWidth = HeaderControlWidth * 2 + HeaderControlGap;
            Rect foldoutRect = new Rect(rect)
            {
                width = Mathf.Max(0f, rect.width - controlsWidth - HeaderControlGap),
            };

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool expanded = EditorGUI.Foldout(foldoutRect, context.RootProperty.isExpanded,
                    context.Label, true);
                if (changed.changed)
                {
                    context.RootProperty.isExpanded = expanded;
                }
            }
            DrawOverrideRichText(foldoutRect, context.Label, overrideRichTextChunks);

            Rect firstControl = new Rect(rect.xMax - controlsWidth, rect.y,
                HeaderControlWidth, rect.height);
            Rect secondControl = new Rect(firstControl.xMax + HeaderControlGap, rect.y,
                HeaderControlWidth, rect.height);

            if (arrayAttribute.Transpose)
            {
                DrawDimensionControlIMGUI(firstControl, "H", rowCount,
                    value => SetDimensionsIMGUI(context, value, columnCount));
                DrawDimensionControlIMGUI(secondControl, "W", columnCount,
                    value => SetDimensionsIMGUI(context, rowCount, value));
            }
            else
            {
                DrawDimensionControlIMGUI(firstControl, "W", columnCount,
                    value => SetDimensionsIMGUI(context, rowCount, value));
                DrawDimensionControlIMGUI(secondControl, "H", rowCount,
                    value => SetDimensionsIMGUI(context, value, columnCount));
            }
        }

        private static void DrawDimensionControlIMGUI(Rect rect, string dimensionLabel, int value,
            Action<int> setValue)
        {
            Rect labelRect = new Rect(rect.x, rect.y, HeaderDimensionLabelWidth, rect.height);
            EditorGUI.LabelField(labelRect, dimensionLabel);

            Rect reduceRect = new Rect(labelRect.xMax, rect.y, HeaderButtonWidth, rect.height);
            Rect addRect = new Rect(rect.xMax - HeaderButtonWidth, rect.y, HeaderButtonWidth, rect.height);
            Rect valueRect = new Rect(reduceRect.xMax, rect.y,
                Mathf.Max(0f, addRect.x - reduceRect.xMax), rect.height);

            int newValue = value;
            using (new EditorGUI.DisabledScope(value <= 0))
            {
                if (GUI.Button(reduceRect, "-", EditorStyles.miniButtonLeft))
                {
                    newValue = value - 1;
                }
            }

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int editedValue = EditorGUI.DelayedIntField(valueRect, GUIContent.none, value);
                if (changed.changed)
                {
                    newValue = editedValue;
                }
            }

            if (GUI.Button(addRect, "+", EditorStyles.miniButtonRight))
            {
                newValue = value + 1;
            }

            newValue = Mathf.Max(0, newValue);
            if (newValue != value)
            {
                setValue.Invoke(newValue);
            }
        }

        private static void DrawColumnLabelsIMGUI(Rect rect, int columnCount, float cellWidth)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
            };
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                Rect cellRect = GetCellRect(rect, columnIndex, cellWidth);
                EditorGUI.LabelField(cellRect, columnIndex.ToString(), style);
            }
        }

        private float GetRowHeightIMGUI(ElementContextIMGUI context, int rowIndex, int columnCount,
            float cellWidth)
        {
            float height = SaintsPropertyDrawer.SingleLineHeight;
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                SaintsWrapUtils.CellInfoIMGUI cellInfo = GetCellInfoIMGUI(context, rowIndex, columnIndex);
                if (!cellInfo.IsValid)
                {
                    continue;
                }

                cellInfo.Property.isExpanded = true;
                height = Mathf.Max(height, IMGUIRawDraw.GetPropertyHeight(cellInfo.Drawer, GUIContent.none,
                    cellInfo.Property, cellInfo.Attributes, cellInfo.RawType, cellInfo.Info,
                    context.InHorizontalLayout));
            }

            return height;
        }

        private bool DrawRowIMGUI(Rect rect, ElementContextIMGUI context, int rowIndex, int columnCount,
            float cellWidth)
        {
            Rect rowLabelRect = new Rect(rect.x, rect.y, RowLabelWidth, rect.height);
            EditorGUI.LabelField(rowLabelRect, rowIndex.ToString(), EditorStyles.miniLabel);

            bool changed = false;
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                SaintsWrapUtils.CellInfoIMGUI cellInfo = GetCellInfoIMGUI(context, rowIndex, columnIndex);
                Rect cellRect = GetCellRect(rect, columnIndex, cellWidth);
                if (!cellInfo.IsValid)
                {
                    EditorGUI.LabelField(cellRect, "\u2014");
                    continue;
                }

                cellInfo.Property.isExpanded = true;
                Rect useRect = cellInfo.ShouldIndent
                    ? new Rect(cellRect.x + 12f, cellRect.y, Mathf.Max(0f, cellRect.width - 12f), cellRect.height)
                    : cellRect;
                using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    IMGUIRawDraw.OnGUI(cellInfo.Drawer, useRect, cellInfo.Property, cellInfo.Attributes,
                        cellInfo.RawType, GUIContent.none, null, cellInfo.Info,
                        context.InHorizontalLayout, false);
                    changed |= changeCheck.changed;
                }
            }

            return changed;
        }

        private static Rect GetCellRect(Rect rowRect, int columnIndex, float cellWidth)
        {
            return new Rect(
                rowRect.x + RowLabelWidth + columnIndex * (cellWidth + CellGap),
                rowRect.y,
                cellWidth,
                rowRect.height);
        }

        private static float GetCellWidth(float width, int columnCount)
        {
            float gaps = CellGap * Mathf.Max(0, columnCount - 1);
            return Mathf.Max(1f, (width - RowLabelWidth - gaps) / Mathf.Max(1, columnCount));
        }

        private SaintsWrapUtils.CellInfoIMGUI GetCellInfoIMGUI(ElementContextIMGUI context,
            int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || rowIndex >= context.RowsProperty.arraySize)
            {
                return default;
            }

            SerializedProperty row = context.RowsProperty.GetArrayElementAtIndex(rowIndex);
            SerializedProperty cells = row.FindPropertyRelative(SerializedRowsName);
            if (cells == null || columnIndex < 0 || columnIndex >= cells.arraySize)
            {
                return default;
            }

            SerializedProperty cell = cells.GetArrayElementAtIndex(columnIndex);
            return SaintsWrapUtils.GetCellInfoIMGUI(context.CellWrapType, context.CellField,
                context.CellType, cell, context.CellAttributes, context.HasSerializeReference,
                context.InHorizontalLayout, $"[{rowIndex}, {columnIndex}]");
        }

        private void SetDimensionsIMGUI(ElementContextIMGUI context, int rowCount, int columnCount)
        {
            int newRowCount = Mathf.Max(0, rowCount);
            int newColumnCount = Mathf.Max(0, columnCount);
            bool changed = context.RowsProperty.arraySize != newRowCount;

            context.RowsProperty.arraySize = newRowCount;
            for (int rowIndex = 0; rowIndex < newRowCount; rowIndex++)
            {
                SerializedProperty row = context.RowsProperty.GetArrayElementAtIndex(rowIndex);
                SerializedProperty cells = row.FindPropertyRelative(SerializedRowsName);
                if (cells.arraySize == newColumnCount)
                {
                    continue;
                }

                cells.arraySize = newColumnCount;
                changed = true;
            }

            if (changed)
            {
                ApplyAndTriggerIMGUI(context);
            }
        }

        private void ApplyAndTriggerIMGUI(ElementContextIMGUI context)
        {
            context.RootProperty.serializedObject.ApplyModifiedProperties();
            (string error, int _, object value) =
                Util.GetValue(context.RootProperty, context.Info, context.Parent);
            if (error == "")
            {
                TriggerChangedIMGUI(context.RootProperty, value);
            }
        }

        private static int GetColumnCountIMGUI(ElementContextIMGUI context)
        {
            if (context.RowsProperty.arraySize == 0)
            {
                return 0;
            }

            SerializedProperty firstRow = context.RowsProperty.GetArrayElementAtIndex(0);
            SerializedProperty cells = firstRow.FindPropertyRelative(SerializedRowsName);
            return cells == null ? 0 : Mathf.Max(0, cells.arraySize);
        }

        private bool TryBuildElementContextIMGUI(SerializedProperty property, GUIContent label,
            FieldInfo info, object parent, out ElementContextIMGUI context)
        {
            context = null;
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            bool insideArray = arrayIndex != -1;
            Type rawType = insideArray ? ReflectUtils.GetElementType(info.FieldType) : info.FieldType;
            if (rawType == null)
            {
                return false;
            }

            string propNameCompact = GetPropName(rawType);
            SerializedProperty rowsProperty = FindPropertyCompact(property, propNameCompact);
            if (rowsProperty == null)
            {
                return false;
            }

            object fieldValue = info.GetValue(parent);
            if (insideArray && fieldValue is IEnumerable enumerable)
            {
                fieldValue = enumerable.Cast<object>().ElementAt(arrayIndex);
            }
            if (fieldValue == null)
            {
                return false;
            }

            (FieldInfo rowsField, object _) = GetTargetInfo(propNameCompact, rawType, fieldValue);
            Type rowType = rowsField == null ? null : ReflectUtils.GetElementType(rowsField.FieldType);
            FieldInfo cellField = GetField(rowType, SerializedRowsName);
            Type cellType = cellField == null ? null : ReflectUtils.GetElementType(cellField.FieldType);
            if (cellField == null || cellType == null)
            {
                return false;
            }

            bool hasSerializeReference = false;
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
                    cellAttributes.Add(cellInjectAttribute);
                    continue;
                }

                Attribute createdAttribute = SaintsWrapUtils.CreateInjectedAttribute(cellInjectAttribute);
                if (createdAttribute != null)
                {
                    cellAttributes.Add(createdAttribute);
                }
            }

            string labelText = string.IsNullOrEmpty(label?.text) ? GetPreferredLabel(property) : label.text;
            GUIContent useLabel = new GUIContent(label)
            {
                text = string.IsNullOrEmpty(labelText) ? "Value" : labelText,
            };

            context = new ElementContextIMGUI
            {
                RootProperty = property,
                RowsProperty = rowsProperty,
                CellField = cellField,
                CellType = cellType,
                CellWrapType = SaintsWrapUtils.EnsureWrapType(
                    property.FindPropertyRelative(SerializedWrapTypeName), cellField, hasSerializeReference),
                HasSerializeReference = hasSerializeReference,
                CellAttributes = cellAttributes,
                Info = info,
                Parent = parent,
                Label = useLabel,
                InHorizontalLayout = InHorizontalLayout,
            };
            return true;
        }
    }
}
