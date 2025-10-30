using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SerNotSupported : SaintsMonoBehaviour
    {
        [SaintsSerialized] public int letUnitySerInt;
        [SaintsSerialized] public string letUnitySerString;

        [Serializable]
        public partial struct CustomType<T>
        {
            [SaintsSerialized]
            public T value;
        }

        public CustomType<DateTime> customDt;
        public CustomType<int> customInt;

        [Space]

        // ReSharper disable InconsistentNaming

        [SaintsSerialized] public IInterface1 interfaceType;
        [SaintsSerialized] public Dictionary<int, IInterface1> dictType;
        [SaintsSerialized] public HashSet<string> hashSetType;
        [SaintsSerialized] public DateTime dateTimeType;
        [SaintsSerialized] public TimeSpan timeSpanType;

    }
}
