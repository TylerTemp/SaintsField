using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    // Note the `partial`!
    public partial class SerInterfaceExample : SaintsMonoBehaviour
    {
        [SaintsSerialized] private IInterface1 _interface1;

        // Use inside class/struct, you need to set as `partial`, together with all it's container
        [Serializable]
        public partial struct SerInterfaceStruct
        {
            [SaintsSerialized] private IInterface1 _interface1InStruct;
        }

        public SerInterfaceStruct structWithInterface;

        [SaintsSerialized] private IInterface1[] _interface1Arr;
        [SaintsSerialized] private List<IInterface1> _interface1Lis;
    }
}
