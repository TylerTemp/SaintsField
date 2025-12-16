using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    // Note the `partial`!
    public partial class SerInterfaceExample : SaintsMonoBehaviour
    {
        [OnValueChanged(nameof(ChangedInterface))]
        [SaintsSerialized] private IInterface1 _interface1;

        private void ChangedInterface(IInterface1 inter) => Debug.Log($"changed: {inter}");

        // Use inside class/struct, you need to set as `partial`, together with all it's container
        [Serializable]
        [StructLayout(LayoutKind.Auto)]
        public partial struct SerInterfaceStruct
        {
            [SaintsSerialized] private IInterface1 _interface1InStruct;
        }

        public SerInterfaceStruct structWithInterface;

        [SaintsSerialized] private IInterface1[] _interface1Arr;
        [SaintsSerialized] private List<IInterface1> _interface1Lis;
    }
}
