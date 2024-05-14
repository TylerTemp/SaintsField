using System;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class CustomEventExample : MonoBehaviour
    {
        public UnityEvent<int, int> intIntEvent;

        [OnEvent(nameof(intIntEvent), value: 1)]
        [OnEvent(nameof(intEvent))]
        [OnEvent(nameof(intEvent), value: 1)]
        public void OnInt1(int int1)  // this is static parameter binding
        {
        }

        [OnEvent(nameof(intIntEvent))]
        public void OnInt2(int int1, int int2)  // dynamic
        {
        }

        public UnityEvent<int> intEvent;

        // can not bind this one
        // // [OnEvent(nameof(intIntEvent), value: 3)]
        // public void OnInt3(int int1, int int2, int int3)
        // {
        // }
    }
}
