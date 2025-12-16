using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SubSerPartExample : MonoBehaviour
    {
        [Serializable]
        public partial class NormalClass
        {
            [SaintsSerialized] private Dictionary<int, string> _simpleDict;
        }

        public NormalClass normalClass;
    }
}
