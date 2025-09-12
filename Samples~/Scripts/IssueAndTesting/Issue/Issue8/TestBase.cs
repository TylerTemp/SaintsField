using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class TestBase : MonoBehaviour
    {
        public bool haveInfo;
        [ShowIf(nameof(haveInfo))]public string infoTextureName;

        public bool isInt;
        [FieldShowIf(nameof(isInt))] public int intA;
    }
}
