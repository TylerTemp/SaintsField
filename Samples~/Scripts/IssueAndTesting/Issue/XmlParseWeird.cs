using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class XmlParseWeird : MonoBehaviour
    {
        [RichLabel("<field=\">><color=yellow>{0}</color><<\"/>")]
        public string[] sindices;
    }
}
