using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue441
{
    // [CreateAssetMenu(fileName = "TestSliderSO", menuName = "Scriptable Objects/TestSliderSO")]
    public class TestSliderSO : ScriptableObject
    {
        public int a;
        [MinMaxSlider(1, 32)] public Vector2Int TestSlider;
        [MinMaxSlider(1, 32)] public Vector2Int TestSlider2;
    }
}
