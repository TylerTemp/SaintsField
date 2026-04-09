using System;
using System.Collections;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ButtonResultIcon : SaintsMonoBehaviour
    {
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
            yield return new WaitForSecondsRealtime(2);  // same as WaitForSeconds
            throw new Exception("Stop There!");
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
