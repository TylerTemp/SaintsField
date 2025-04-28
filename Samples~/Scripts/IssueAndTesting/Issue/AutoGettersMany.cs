using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class AutoGettersMany : MonoBehaviour
    {
        [GetByXPath(EXP.JustPicker, "assets:://*")]
        [GetByXPath("scene:://*")]
        public GameObject anyObj;
    }
}
