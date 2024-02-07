using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue318
{
    public class Sub1 : RootClass
    {
        [Layout("RootGroup")] public int sub1ToRoot;

        public string sub1Item1;

        [Layout("Sub1Group", ELayout.Title | ELayout.Background)] public int sub1Group1, sub1Group2;

        public string sub1ItemAfterGroup;
    }
}
