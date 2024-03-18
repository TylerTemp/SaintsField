using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class TestBase : MonoBehaviour
    {
        public bool haveInfo;
        [PlayaShowIf(nameof(haveInfo))]public string infoTextureName;

        public bool isInt;
        [ShowIf(nameof(isInt))] public int intA;
    }
}
