using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue62;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;
using MCDamageHandler = SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue270.MCDamageHandler;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue132
{
    public class Issue132Base : SaintsMonoBehaviour
    {
        public virtual MCDamageHandler realSelf => null;
        public virtual bool            isRelay  => false;

        public virtual MCTeam          team   => null;
    }
}
