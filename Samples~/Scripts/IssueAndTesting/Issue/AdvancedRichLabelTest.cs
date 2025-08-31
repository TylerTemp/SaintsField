using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class AdvancedRichLabelTest : MonoBehaviour
    {
        [AdvancedDropdown(nameof(Options))]
        public GameObject advTest;

        [GetComponentInChildren] public GameObject[] go;

        private AdvancedDropdownList<GameObject> Options() => new AdvancedDropdownList<GameObject>
        {
            {"<color=red>Root</color> Option/1", go[0]},
            {"<color=red>Root</color> Option/<color=brown>2", go[1]},
            {"<color=red>Root</color> Option/3", go[2]},
            {"<color=lime>Sub/1", go[3]},
            {"<color=lime>Sub/<b>2</b>", go[4]},
            {"<color=lime>Sub/3", go[5]},
            {"<color=lime>Sub/<color=blue>4", go[6]},
            {"<color=lime>Sub/5", go[7]},
        };

        [Serializable]
        public enum MyEnum
        {
            [RichLabel("<color=red>Root</color> Option/1")]
            E1,
            [RichLabel("<color=red>Root</color> Option/<color=brown>2")]
            E2,
            [RichLabel("<color=red>Root</color> Option/3")]
            E3,
            [RichLabel("<color=lime>Sub/1")]
            E4,
            [RichLabel("<color=lime>Sub/<b>2</b>")]
            E5,
            [RichLabel("<color=lime>Sub/3")]
            E6,
            [RichLabel("<color=lime>Sub/<color=blue>4")]
            E7,
            [RichLabel("<color=lime>Sub/5")]
            E8,
        }

        [AdvancedDropdown]
        public MyEnum myEnum;

        [EnumToggleButtons] public MyEnum myEnumBt;
    }
}
