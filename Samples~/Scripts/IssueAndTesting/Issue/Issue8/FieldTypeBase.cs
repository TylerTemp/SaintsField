using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class FieldTypeBase : MonoBehaviour
    {
        [SerializeField, FieldType(typeof(Collider))]
        private Dummy dummy;
    }
}
