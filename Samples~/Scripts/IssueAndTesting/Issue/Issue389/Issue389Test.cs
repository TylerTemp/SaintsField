using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue389
{
    public class Issue389Test : SaintsMonoBehaviour
    {
        [DefaultExpand]
        public NotMatchedContainer notMatched;
        [DefaultExpand]
        public BuildObject matched;

        [Serializable]
        public struct DownWalkProp
        {
            public string myContent;
        }

        [AboveText("<field.myContent/>")]
        public DownWalkProp v;

        // same as
        [AboveText("$field::" + nameof(DownWalkProp.myContent))]
        public DownWalkProp v2;

        public bool curFieldV;
        [EnableIf("$" + nameof(curFieldV))]
        public string isOn;

        public int myInt;

        [EnableIf(nameof(myInt), 2)] public int enableIfEquals2;
    }
}
