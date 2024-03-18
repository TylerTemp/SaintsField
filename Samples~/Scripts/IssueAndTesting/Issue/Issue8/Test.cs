using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class Test : TestBase
    {
        [Space(20)]
        public bool isString;
        [ShowIf("isString")]
        public string strA;

        public bool haveInfo2;
        [PlayaShowIf(nameof(haveInfo2))] public string infoTextureName2;
    }
}
