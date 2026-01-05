using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue11
{
    public class Issue11AndOr : MonoBehaviour
    {
        public bool bool1;
        public bool bool2;

        // disable==1&&2
        [FieldDisableIf(nameof(bool1), nameof(bool2))]
        public string disable1And2;
        // disable==1||2
        [FieldDisableIf(nameof(bool1)), FieldDisableIf(nameof(bool2))]
        public string disable1Or2;

        // enable==1&&2, which means disable==!(1&&2)==!1||!2
        [FieldEnableIf(nameof(bool1)), FieldEnableIf(nameof(bool2))]
        public string enable1And2;
        // enable==1||2, which means disable==!(1||2)==!1&&!2
        [FieldEnableIf(nameof(bool1), nameof(bool2))]
        public string enable1Or2;
    }
}
