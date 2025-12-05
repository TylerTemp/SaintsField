using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue346LabelTestEditor : MonoBehaviour
    {
        [Serializable]
        public struct TestStruct
        {
            [EndText("seconds")] public int secondsC;
            [EndText("seconds")] public int[] secondsCArr;

            [OverlayText("seconds")] public int secondsD;
            [OverlayText("seconds")] public int[] secondsDArr;
        }

        public TestStruct testStruct;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Issue346LabelTestEditor))]
    public class LabelTestEditor : SaintsField.Editor.SaintsEditor  // <-- Use this
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            VisualElement defaultFields = base.CreateInspectorGUI();
            root.Add(defaultFields);

            // If you want to use IMGUI, put it inside this
            root.Add(new IMGUIContainer(() =>
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.TextField("IMGUI Text Field");
                    GUILayout.Button("IMGUI Button");
                }
            }));

            return root;
        }
    }
#endif
}
