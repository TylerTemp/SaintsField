#if UNITY_EDITOR
using System.Collections;
using SaintsField.Editor;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Samples.EditorTest
{
    public class EditorWindowOrder : SaintsEditorWindow
    {
        public string targetDirectory = "Assets";
        public int newMaxSize = 128;

        [ProgressBar(0, maxCallback: nameof(_maxCount)), NoLabel, FieldReadOnly]
        public int progressBar;

        private int _maxCount = 100;

#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/EditorWindowOrder")]
#endif
        public static void ShowWindow()
        {
            GetWindow<EditorWindowOrder>();
        }

        [Button("ApplyAndroid")]
        private void ApplyMaxSizeToAndroidTextures()
        {
        }

        [Button("ApplyIOS")]
        private IEnumerator ApplyMaxSizeToAndroidTexturesIOS()
        {
            yield break;
        }
    }
}
#endif
