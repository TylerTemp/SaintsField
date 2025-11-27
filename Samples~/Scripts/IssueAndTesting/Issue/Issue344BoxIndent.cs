using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor.Issues;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue344BoxIndent : SaintsMonoBehaviour
    {
        [Serializable]
        public struct GatheringFromWorld
        {
            public Issue296Table.Gathering gathering;
            public string n;
            [LayoutStart("Damage", ELayout.FoldoutBox)]
            public string damage;
            [LayoutStart("Health", ELayout.FoldoutBox)]
            public string health;
            [LayoutStart("Size", ELayout.FoldoutBox)]
            public string size;
        }

        [InfoBox("The Indent is actually correct, foldout is always negative left margin. Unity Design!")]
        public GatheringFromWorld gatheringFromWorld;
    }
}
