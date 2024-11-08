using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue94 : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(0, 20f)]
        internal Vector2 range = new Vector2(2, 10);
    }
}
