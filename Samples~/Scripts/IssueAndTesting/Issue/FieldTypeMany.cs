using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class FieldTypeMany : MonoBehaviour
    {
        [FieldType(typeof(Transform))] public GameObject anyObj;
    }
}
