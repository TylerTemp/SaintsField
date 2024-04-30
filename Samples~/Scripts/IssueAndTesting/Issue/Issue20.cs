using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue20 : MonoBehaviour
    {
        [Flags]
        public enum Bldg_DestructState
        {
            NONE = 0,
            TOP_LEFT = 1, TOP_RIGHT = 2,
            BOT_LEFT = 4, BOT_RIGHT = 8,
            ADJ_LEFT = 16, ADJ_RIGHT = 32,
            FOUR = TOP_LEFT | TOP_RIGHT | BOT_LEFT | BOT_RIGHT,
        }

        [BelowRichLabel(nameof(wrapped), true)]
        [EnumFlags] public Bldg_DestructState wrapped;
        [BelowRichLabel(nameof(original), true)]
        public Bldg_DestructState original;

        [EnumFlags(autoExpand: false, defaultExpanded: false)] public Bldg_DestructState noAutoDefaultNoExpand;
        [EnumFlags(autoExpand: false, defaultExpanded: true)] public Bldg_DestructState noAutoDefaultExpanded;
        [EnumFlags(autoExpand: true, defaultExpanded: false)] public Bldg_DestructState autoDefaultNoExpand;
        [EnumFlags(autoExpand: true, defaultExpanded: true)] public Bldg_DestructState autoDefaultExpanded;
    }
}
