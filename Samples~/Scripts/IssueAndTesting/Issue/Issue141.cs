using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue41;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using UnityEngine;
using MCUnit = SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue270.MCUnit;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue141: MonoBehaviour
    {
        public MCTeam team;
        [GetComponentInParents]
        public MCUnit self;
    }
}
