using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.RendererGroup;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.Table
{
    public partial class TableRenderer
    {
        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            int arraySize = FieldWithInfo.SerializedProperty.arraySize;
            if (arraySize == 0)
            {
                if(FieldWithInfo.SerializedProperty.isExpanded)
                {
                    return SaintsPropertyDrawer.SingleLineHeight * 3 + 8;
                }

                return SaintsPropertyDrawer.SingleLineHeight;
            }

            return FieldWithInfo.SerializedProperty.isExpanded
                ? EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(0), true)
                : EditorGUIUtility.singleLineHeight;
        }

        protected override void SerializedFieldRenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {

            bool needIndent = SaintsRendererGroup.IMGUINeedIndentFix && (FieldWithInfo.SerializedProperty.isArray ||
                                                                         FieldWithInfo.SerializedProperty
                                                                             .propertyType ==
                                                                         SerializedPropertyType.Generic);

            GUIContent useGUIContent = preCheckResult.HasRichLabel
                ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length), tooltip: FieldWithInfo.SerializedProperty.tooltip)
                : new GUIContent(FieldWithInfo.SerializedProperty.displayName, tooltip: FieldWithInfo.SerializedProperty.tooltip);

            using(new EditorGUI.IndentLevelScope(needIndent? 1: 0))
            {
                int arraySize = FieldWithInfo.SerializedProperty.arraySize;
                if (arraySize == 0)
                {
                    EditorGUI.PropertyField(position, FieldWithInfo.SerializedProperty, useGUIContent);
                }
                else
                {
                    (Rect foldout, Rect left) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

                    bool expanded = FieldWithInfo.SerializedProperty.isExpanded =
                        EditorGUI.Foldout(foldout, FieldWithInfo.SerializedProperty.isExpanded, useGUIContent, true);
                    if(expanded)
                    {
                        EditorGUI.PropertyField(
                            left,
                            FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(0), useGUIContent);
                    }

                    DragAndDropImGui(foldout, ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ?? FieldWithInfo.PropertyInfo.PropertyType), FieldWithInfo.SerializedProperty);
                }
            }
        }

        public override void OnDestroy()
        {
        }

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            float height = GetFieldHeightIMGUI(width, preCheckResult);
            Rect position = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
            RenderPositionTargetIMGUI(position, preCheckResult);
        }
    }
}
