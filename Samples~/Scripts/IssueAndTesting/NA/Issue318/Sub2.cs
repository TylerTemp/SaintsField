using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue318
{
    public class Sub2: RootClass
    {
        [Layout("RootGroup")] public int sub2ToRoot;

        public string sub2Item1;

        [Layout("Sub2GroupOnTop", ELayout.Title | ELayout.Background)] public int sub2Group1, sub2Group2;

        public string sub2ItemAfterGroup;
    }
}
