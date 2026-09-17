using System;
using System.Collections;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class OnInspectorDisposeExample : SaintsMonoBehaviour
    {
        public bool error;

        [OnInspectorDispose]
        private void OnInspectorDispose()
        {
            if (error)
            {
                throw new Exception($"Expected Error! {GetId()}");
            }
            Debug.Log($"Dispose done {GetId()}");
        }

        [OnInspectorDispose]
        private IEnumerator OnInspectorAsync()
        {
            string id = GetId();

            yield return new WaitForSeconds(2);
            Debug.Log($"Dispose async done {id}");
        }

        private string GetId()
        {
#if UNITY_6000_4_OR_NEWER
            return GetEntityId().ToString();
#else
            return GetInstanceID().ToString();
#endif
        }
    }
}
