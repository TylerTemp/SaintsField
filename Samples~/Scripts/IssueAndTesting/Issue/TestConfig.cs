using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class TestConfig : MonoBehaviour
    {
        [GetComponent] public Dummy dummy;
        [ResizableTextArea] public string textArea;
    }
}
