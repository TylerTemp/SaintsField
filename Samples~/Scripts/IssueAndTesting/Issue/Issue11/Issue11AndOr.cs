using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue11
{
    public class Issue11AndOr : MonoBehaviour
    {
        public bool bool1;
        public bool bool2;

        // disable==1&&2
        [DisableIf(nameof(bool1), nameof(bool2))]
        public string disable1And2;
        // disable==1||2
        [DisableIf(nameof(bool1)), DisableIf(nameof(bool2))]
        public string disable1Or2;

        // enable==1&&2, which means disable==!(1&&2)==!1||!2
        [EnableIf(nameof(bool1)), EnableIf(nameof(bool2))]
        public string enable1And2;
        // enable==1||2, which means disable==!(1||2)==!1&&!2
        [EnableIf(nameof(bool1), nameof(bool2))]
        public string enable1Or2;
    }
}
