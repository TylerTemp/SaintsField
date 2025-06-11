using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class MultipleButton : MonoBehaviour
    {
        [PostFieldButton(nameof(Click))] public string s;

        private void Click(string v) => Debug.Log(v);
    }
}
