using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;
using Object = UnityEngine.Object;
using static SaintsField.Samples.Scripts.SaintsEditor.Testing.FixUsingStatic;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public partial class FixUsing: SaintsMonoBehaviour
    {
        [SaintsSerialized] private IInterface1 _interface1;

        private void Awake()
        {
            Debug.Log(typeof(Object));
            FunctionHere();
        }
    }
}
