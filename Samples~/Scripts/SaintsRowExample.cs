using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SaintsRowExample : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string normalField;

            [PlayaRichLabel("<color=green><label/>")]
            public string[] myStrings;
        }

        [SaintsRow] public MyStruct myStruct;
    }
}
