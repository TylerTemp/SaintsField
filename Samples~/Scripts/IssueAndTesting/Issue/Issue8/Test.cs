using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class Test : TestBase
    {
        [SepTitle("Inherent:Test", EColor.Gray)]
        public bool isString;
        [FieldShowIf("isString")]
        public string strA;

        public bool haveInfo2;
        [ShowIf(nameof(haveInfo2))] public string infoTextureName2;
    }
}
