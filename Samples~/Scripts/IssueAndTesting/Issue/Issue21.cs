using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue21 : MonoBehaviour
    {
        public string beforeBox1;

        [Layout("The Lonesome Crowded West", ELayout.Background | ELayout.Title | ELayout.TitleOut)]
        public string s1;
        [Layout("The Lonesome Crowded West")]
        public string s2;

        public string afterBox1;

        [SepTitle(EColor.Gray)]

        public string beforeBox2;

        [Layout("Strangers to Ourselves", ELayout.Background | ELayout.Title | ELayout.TitleOut | ELayout.Foldout)]
        public string s3;
        [Layout("Strangers to Ourselves")]
        public string s4;

        public string afterBox2;
    }
}
