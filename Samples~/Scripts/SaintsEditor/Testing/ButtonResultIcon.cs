using System;
using System.Collections;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ButtonResultIcon : SaintsMonoBehaviour
    {
        public string s1;
        [Button]
        private int ButtonParamsReturn(int p)
        {
            return p;
        }

        [Button]
        private void ButtonParams(int p)
        {

        }

        [Button]
        private int ButtonReturn()
        {
            return 1;
        }
        public string s2;

        [Button]
        private void NormalFunc()
        {

        }
        [Button]
        private void ErrorFunc()
        {
            throw new Exception("Stop There!");
        }
        [Button]
        private IEnumerator IEFunc()
        {
            yield return new WaitForSeconds(4);  // Note: Pausing the editor will NOT pause this enumerator
        }

        [Button]
        private IEnumerator IEFuncError()
        {
            float waitTime = UnityEngine.Random.Range(0.5f, 2f);
            yield return new WaitForSecondsRealtime(waitTime);  // same as WaitForSeconds
            throw new Exception($"Expected error happend after {waitTime}s");
        }

        [ShowInInspector] private bool _waitUntilMe;

        [Button]
        private IEnumerator WaitUntilChecked()
        {
            yield return new WaitUntil(() => _waitUntilMe);
        }

        [ShowInInspector] private bool _waitWhileMe = true;

        [Button]
        private IEnumerator WaitWhileChecked()
        {
            yield return new WaitWhile(() => _waitWhileMe);
        }

        [ResourcePath(EStr.Resource, typeof(GameObject))]
        public string res;

        [Button]  // Loader using `AsyncOperation` is supported
        private IEnumerator AsyncOp()
        {
            AsyncOperation op = Resources.LoadAsync(res);
            yield return op;
            Debug.Log("DONE");
        }
    }
}
