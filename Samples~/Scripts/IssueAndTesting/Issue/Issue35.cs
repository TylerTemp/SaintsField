using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue35 : MonoBehaviour
    {
        [MinMaxSlider(-1, 10)] public Vector2 _buggySync;
    }
}
