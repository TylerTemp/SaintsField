using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    // Note the `partial`!
    public partial class SerDictionaryExample : SaintsMonoBehaviour
    {
        [OnValueChanged(nameof(SimpleDictChangedWithValue))]
        [OnValueChanged(nameof(DontCareActualValue))]
        [SaintsSerialized] private Dictionary<int, string> simpleDict;

        [ShowInInspector] private Dictionary<int, string> Show => simpleDict;

        // If you want to receive the value, you need to use `SaintsDictionary` instead of `Dictionary`
        private void SimpleDictChangedWithValue(SaintsDictionary<int, string> dict)
        {
            Debug.Log($"dict changed: {string.Join("; ", dict.Select(each => $"{each.Key}={each.Value}"))}");
        }

        private void DontCareActualValue()
        {
            Debug.Log($"value changed: {string.Join("; ", simpleDict.Select(each => $"{each.Key}={each.Value}"))}");
        }

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
