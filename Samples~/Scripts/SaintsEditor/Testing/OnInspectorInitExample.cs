using System;
using System.Collections;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class OnInspectorInitExample : SaintsMonoBehaviour
    {
        public bool error;

#pragma warning disable CS0414 // Field is assigned but its value is never used
        [ShowInInspector] private bool _ran;
#pragma warning restore CS0414 // Field is assigned but its value is never used

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            _ran = true;

            if (error)
            {
                throw new Exception($"Expected Error! {GetId()}");
            }
            Debug.Log($"Init done {GetId()}");
        }

#pragma warning disable CS0414 // Field is assigned but its value is never used
        [ShowInInspector] private bool _ranAsync;
#pragma warning restore CS0414 // Field is assigned but its value is never used

        [OnInspectorInit]
        private IEnumerator OnInspectorAsync()
        {
            yield return new WaitForSeconds(2);
            _ranAsync = true;
            Debug.Log($"Async done {GetId()}");
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
