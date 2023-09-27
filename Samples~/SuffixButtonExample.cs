using UnityEngine;
using ExtInspector;
using ExtInspector.Utils;

namespace ExtInspector.Samples
{
    public class SuffixButtonExample: MonoBehaviour
    {
#if UNITY_EDITOR
        [SuffixButton(nameof(EditorExampleCallback), " OK", Icon.Eye)]
#endif
        public string example;

#if UNITY_EDITOR
        [SuffixButton(nameof(EditorExampleCallback), null, Icon.Eye)]
#endif
        public string example2;

#if UNITY_EDITOR
        [SuffixButton(nameof(EditorExampleCallback), "Click Me!")]
#endif
        public string example3;

#if UNITY_EDITOR
        private void EditorExampleCallback() {
            Debug.Log("EditorExampleCallback");
        }
#endif
    }
}
