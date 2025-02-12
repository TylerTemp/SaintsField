using System.Linq;
using SaintsField.Editor.Playa.RendererGroup;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.Table
{
    public partial class TableRenderer
    {
        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            int arraySize = FieldWithInfo.SerializedProperty.arraySize;
            return arraySize == 0
                ? EditorGUIUtility.singleLineHeight * 2
                : EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(0), true);
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
                    EditorGUILayout.PropertyField(FieldWithInfo.SerializedProperty, useGUIContent,
                        GUILayout.ExpandWidth(true));
                }
                else
                {
                    FieldWithInfo.SerializedProperty.isExpanded =
                        EditorGUILayout.Foldout(FieldWithInfo.SerializedProperty.isExpanded, useGUIContent);
                    if(FieldWithInfo.SerializedProperty.isExpanded)
                    {
                        EditorGUILayout.PropertyField(
                            FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(0), useGUIContent,
                            GUILayout.ExpandWidth(true));
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
            Rect position = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(true));
            RenderPositionTargetIMGUI(position, preCheckResult);
        }
    }
}
