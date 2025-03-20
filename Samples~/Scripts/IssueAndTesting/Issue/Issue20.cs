using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue20 : MonoBehaviour
    {
        [Flags]
        // ReSharper disable InconsistentNaming
        public enum Bldg_DestructState
        {
            NONE = 0,
            TOP_LEFT = 1, TOP_RIGHT = 2,
            BOT_LEFT = 4, BOT_RIGHT = 8,
            ADJ_LEFT = 16, ADJ_RIGHT = 32,
            FOUR = TOP_LEFT | TOP_RIGHT | BOT_LEFT | BOT_RIGHT,
        }
        // ReSharper restore InconsistentNaming

        [BelowRichLabel(nameof(wrapped), true)]
        [EnumToggleButtons] public Bldg_DestructState wrapped;
        [BelowRichLabel(nameof(original), true)]
        public Bldg_DestructState original;

        [EnumToggleButtons] public Bldg_DestructState autoDefaultNoExpand;
        [EnumToggleButtons, DefaultExpand] public Bldg_DestructState autoDefaultExpanded;
    }
}
