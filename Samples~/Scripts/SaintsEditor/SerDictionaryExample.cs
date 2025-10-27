using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    // Note the `partial`!
    public partial class SerDictionaryExample : SaintsMonoBehaviour
    {
        // public SaintsDictionary<int, IInterface1> dictInterfaceSaints;
        //
        // // [ShowInInspector] private Dictionary<int, IInterface1> d => dictInterfaceSaints;
        //
        // [Button]
        // private IInterface1 GetKey(int k)
        // {
        //     // Debug.Log(dictInterfaceSaints[k]);
        //     return dictInterfaceSaints[k];
        // }

        [Serializable]
        public struct Sub
        {
            public string subString;
            public int subInt;

            public override string ToString()
            {
                return $"<Sub {subString} {subInt}/>";
            }
        }

        [SaintsSerialized] public Dictionary<int, Sub> dictIntToStruct;
        [SaintsSerialized] private Dictionary<int, IInterface1> _dictInterface;

        [SaintsSerialized] private Dictionary<IInterface1, int>[] _dictInterfaceArr;
        [SaintsSerialized] private List<Dictionary<IInterface1, IInterface1>> _dictInterfaceLis;
    }
}
