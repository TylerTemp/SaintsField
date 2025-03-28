using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldBareRenderer
    {
        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty, true);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            EditorGUI.PropertyField(position, FieldWithInfo.SerializedProperty, new GUIContent(FieldWithInfo.SerializedProperty.displayName), true);
        }


        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            EditorGUILayout.PropertyField(FieldWithInfo.SerializedProperty, new GUIContent(FieldWithInfo.SerializedProperty.displayName), true);
        }
    }
}
