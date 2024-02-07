using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue318
{
    public class RootClass : MonoBehaviour
    {
        public string root1;
        public string root2;

        [Layout("RootGroup", ELayout.Title | ELayout.Background | ELayout.TitleOut)]
        public int firstInt;
        [Layout("RootGroup")]
        public int secondInt;

        public string rootAfterGroup;
    }
}
