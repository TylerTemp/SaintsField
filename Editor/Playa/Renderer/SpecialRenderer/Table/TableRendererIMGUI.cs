using System.Linq;
using SaintsField.Editor.Playa.RendererGroup;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.Table
{
    public partial class TableRenderer
    {
        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            int arraySize = FieldWithInfo.SerializedProperty.arraySize;
            if (arraySize == 0)
                return EditorGUIUtility.singleLineHeight * 2;
            return FieldWithInfo.SerializedProperty.isExpanded
                ? EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(0), true)
                : EditorGUIUtility.singleLineHeight;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            // TableAttribute tableAttribute = FieldWithInfo.PlayaAttributes.OfType<TableAttribute>().First();

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

                    FieldWithInfo.SerializedProperty.isExpanded =
                        EditorGUI.Foldout(foldout, FieldWithInfo.SerializedProperty.isExpanded, useGUIContent);
                    if(FieldWithInfo.SerializedProperty.isExpanded)
                    {
                        EditorGUI.PropertyField(
                            left,
                            FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(0), useGUIContent);
                    }
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
