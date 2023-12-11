using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class PostFieldButtonExample: MonoBehaviour
    {
#if UNITY_EDITOR
        [PostFieldButton(nameof(EditorExampleCallback), "<color=yellow><icon='eye.png' /></color> OK")]
#endif
        public string example;

#if UNITY_EDITOR
        [PostFieldButton(nameof(EditorExampleCallback), "<color=green><icon='eye.png' /></color>")]
#endif
        public string example2;

#if UNITY_EDITOR
        [PostFieldButton(nameof(EditorExampleCallback), "Click Me!")]
#endif
        public string example3;

#if UNITY_EDITOR
        private void EditorExampleCallback() {
            Debug.Log("EditorExampleCallback");
        }
#endif
    }
}
