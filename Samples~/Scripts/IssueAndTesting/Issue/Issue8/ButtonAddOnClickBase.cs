using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ButtonAddOnClickBase : MonoBehaviour
    {
        [SerializeField, FieldType(typeof(Button))] protected GameObject buttonTarget;
        public void ButtonClick() {}
    }
}
