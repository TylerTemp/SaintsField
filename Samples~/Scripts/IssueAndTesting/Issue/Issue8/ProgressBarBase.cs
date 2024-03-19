using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ProgressBarBase : MonoBehaviour
    {
        [Space]
        public int minValue;
        public int maxValue;

        protected EColor BackgroundColor(float v) => v <= 0? EColor.Brown: EColor.CharcoalGray;

        protected Color FillColor(float v) => Color.Lerp(Color.yellow, EColor.Green.GetColor(), Mathf.Pow(Mathf.InverseLerp(minValue, maxValue, v), 2));

        protected string Title(float curValue, float min, float max, string label) => curValue < 0 ? $"[{label}] Game Over: {curValue}" : $"[{label}] {curValue / max:P}";

    }
}
