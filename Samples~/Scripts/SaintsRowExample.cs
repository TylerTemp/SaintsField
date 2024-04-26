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
            // public string normalField;

            [PlayaRichLabel("<color=green><icon=star.png/><label/>")]
            public string[] myStrings;
        }

        [SaintsRow] public MyStruct myStruct;
        [SaintsRow(inline: true)] public MyStruct myStructInline;
    }
}
