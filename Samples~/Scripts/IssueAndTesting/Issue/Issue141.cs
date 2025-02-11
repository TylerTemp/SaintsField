using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue41;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue141: MonoBehaviour
    {
        public MCTeam team;
        [GetComponentInParents]
        public MCUnit self;
    }
}
