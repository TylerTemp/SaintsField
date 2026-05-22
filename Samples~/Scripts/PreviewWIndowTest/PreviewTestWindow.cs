#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.PreviewWIndowTest
{
    public class PreviewTestWindow : EditorWindow
    {
        Object targetObject;
        UnityEditor.Editor cachedEditor;

        [MenuItem("Saints Field/Preview Test")]
        static void Open()
        {
            GetWindow<PreviewTestWindow>();
        }

        void OnGUI()
        {
            targetObject = EditorGUILayout.ObjectField(
                "Target",
                targetObject,
                typeof(Object),
                true);

            if (targetObject == null)
                return;

            UnityEditor.Editor.CreateCachedEditor(
                targetObject,
                null,
                ref cachedEditor);

            if(cachedEditor.HasPreviewGUI())
            {
                {
                    Rect rect = GUILayoutUtility.GetRect(300, 300);

                    // cachedEditor.OnPreviewGUI(
                    //     rect,
                    //     EditorStyles.helpBox);
                    cachedEditor.OnInteractivePreviewGUI(rect, EditorStyles.helpBox);
                }
            }

        }
    }
}
#endif
