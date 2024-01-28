using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting
{
    public class DupDecImGui : MonoBehaviour
    {
        [SepTitle(EColor.Green)]
        [InfoBox("one SepTitle")]
        [OnValueChanged(nameof(Changed))]
        [Range(0, 10)]
        public int value;

        [SepTitle("One", EColor.Green)]
        [Header("One Header")]
        [SepTitle("Two", EColor.Green)]
        [Header("Two Header")]
        [InfoBox("two SepTitle")]
        [OnValueChanged(nameof(Changed))]
        [Range(0, 10)]
        public int value2;

        [InfoBox("No SepTitle")]
        [OnValueChanged(nameof(Changed))]
        [Range(0, 10)]
        public int value3;

        [SepTitle(EColor.Green)]
        public int onlySepTitle;

        private void Changed() => Debug.Log(value);
    }
}
