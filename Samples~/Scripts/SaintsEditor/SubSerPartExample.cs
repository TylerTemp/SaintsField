using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SubSerPartExample : SaintsMonoBehaviour
    {
        [Serializable]
        public partial class NormalClass
        {
            [SaintsSerialized] public Dictionary<int, string> _simpleDict;
        }

        public NormalClass normalClass;

        [ShowInInspector]
        public Dictionary<int, string> SimpleDict => normalClass._simpleDict;
    }
}
