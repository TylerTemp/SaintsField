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
            foreach (int i in Enumerable.Range(0, 50))
            {
                yield return new WaitForSeconds(1);
                // Debug.Log(i);
            }
        }

        [Button]
        private IEnumerator IEFuncError()
        {
            foreach (int i in Enumerable.Range(0, 50))
            {
                yield return new WaitForSeconds(1);
                if (i > 25)
                {
                    throw new Exception("Stop There!");
                }
                // Debug.Log(i);
            }
        }
    }
}
