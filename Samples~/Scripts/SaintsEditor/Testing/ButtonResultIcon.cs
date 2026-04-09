using System;
using System.Collections;
using System.Linq;
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
            foreach (int i in Enumerable.Range(0, 3))
            {
                Debug.Log(i);
                yield return new WaitForSeconds(1);
            }
        }

        [Button]
        private IEnumerator IEFuncError()
        {
            yield return new WaitForSeconds(1);
            throw new Exception("Stop There!");
        }

        public bool waitUntilMe;

        [Button]
        private IEnumerator WaitUntilChecked()
        {
            yield return new WaitUntil(() => waitUntilMe);
        }

        public bool waitWhileMe = true;

        [Button]
        private IEnumerator WaitWhileChecked()
        {
            yield return new WaitWhile(() => waitWhileMe);
        }

        [ResourcePath(EStr.Resource, typeof(GameObject))]
        public string res;

        [Button]
        private IEnumerator AsyncOp()
        {
            AsyncOperation op = Resources.LoadAsync(res);
            yield return op;
            Debug.Log("DONE");
        }
    }
}
