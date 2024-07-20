using System;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class CustomEventExample : SaintsMonoBehaviour
    {
        public UnityEvent<int, int> intIntEvent;

        [OnEvent(nameof(intIntEvent), value: 1)]
        [OnEvent(nameof(intEvent))]
        public void OnInt1(int int1)  // this is static parameter binding
        {
        }

        [OnEvent(nameof(intIntEvent))]
        public void OnInt2(int int1, int int2)  // dynamic
        {
        }

        public UnityEvent<int> intEvent;

        [SerializeField, GetComponentInChildren]
        private CustomEventChild _child;

        [OnEvent(nameof(_child) + "._intEvent", value: 1)]
        public void OnChildInt(int int1)  // this is static parameter binding
        {
        }

        // can not bind this one
        // // [OnEvent(nameof(intIntEvent), value: 3)]
        // public void OnInt3(int int1, int int2, int int3)
        // {
        // }
    }
}
