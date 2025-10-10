using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue319TypeReferenceShadow : MonoBehaviour
    {
        public class TestComponent {}

        public class SubComponent: TestComponent {}

        // [TypeReference(EType.Current, new []{typeof(TestComponent)})]
        // [SerializeField] private TypeReference _typeRef;

        [ValidateInput(nameof(Vi))]
        [TypeReference(EType.Current, new []{typeof(TestComponent)})]
        [SerializeField] private TypeReference _typeRefShadow;

        private bool Vi() => true;
    }
}
