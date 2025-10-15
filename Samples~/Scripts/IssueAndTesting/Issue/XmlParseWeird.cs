using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class XmlParseWeird : MonoBehaviour
    {
        [FieldLabelText("<field=\">><color=yellow>{0}</color><<\"/>")]
        public string[] sindices;
    }
}
