using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SliderExample)), CanEditMultipleObjects]
public class SliderExampleEditor : Editor
{
    protected virtual void OnSceneGUI()
    {
        SliderExample example = (SliderExample)target;

        float size = HandleUtility.GetHandleSize(example.targetPosition) * 0.5f;
        float snap = 0.1f;

        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPosition = Handles.Slider(example.targetPosition, Vector3.right, size, Handles.ConeHandleCap, snap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(example, "Change Look At Target Position");
            example.targetPosition = newTargetPosition;
            example.Update();
        }
    }
}
