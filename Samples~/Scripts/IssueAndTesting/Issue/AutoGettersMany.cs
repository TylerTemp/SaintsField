using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class AutoGettersMany : MonoBehaviour
    {
        public GameObject go;

        [GetByXPath(EXP.JustPicker, "assets:://*")]
        [GetByXPath("scene:://*")]
        public Object anyObj;
    }
}
