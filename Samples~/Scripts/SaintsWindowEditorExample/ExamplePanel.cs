#if UNITY_EDITOR
using System.Collections;
using SaintsField.Editor;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsWindowEditorExample
{
    public class ExamplePanel: SaintsEditorWindow
    {

#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/SaintsEditor")]
#else
        [MenuItem("Window/Saints/Example/SaintsEditor")]
#endif
        public static void TestOpenWindow()
        {
            EditorWindow window = GetWindow<ExamplePanel>(false, "My Panel");
            window.Show();
        }

        [ResizableTextArea]
        public string myString;

        [ProgressBar(100f)] public float myProgress;

        public override void OnEditorUpdate()
        {
            myProgress = (myProgress + 1f) % 100;
        }

        [ProgressBar(100f)] public float myCoroutine;

        [LayoutStart("Coroutine", ELayout.Horizontal)]

        private IEnumerator _startProcessing;

        [Button]
        public void StartIt()
        {
            StartEditorCoroutine(_startProcessing = StartProcessing());
        }

        [Button]
        public void StopIt()
        {
            if (_startProcessing != null)
            {
                StopEditorCoroutine(_startProcessing);
            }

            _startProcessing = null;
        }

        private IEnumerator StartProcessing()
        {
            myCoroutine = 0;
            while (myCoroutine < 100f)
            {
                myCoroutine += 1f;
                yield return null;
            }
        }

        public override void OnEditorEnable()
        {
            Debug.Log("Enable");
        }

        public override void OnEditorDisable()
        {
            Debug.Log("Disable");
        }

        public override void OnEditorDestroy()
        {
            Debug.Log("Destroy");
        }
    }
}
#endif
