using UnityEngine;

namespace SaintsField
{
    public class ProgressBarExample: MonoBehaviour
    {
        public int minValue;
        public int maxValue;

        [ProgressBar(nameof(minValue), nameof(maxValue), step: 0.05f, backgroundColorCallback: nameof(BackgroundColor), colorCallback: nameof(FillColor), titleCallback: nameof(Title))]
        [RichLabel(null)]
        public float fValue;

        private EColor BackgroundColor()
        {
            return fValue <= 0? EColor.Brown: EColor.CharcoalGray;
        }

        private Color FillColor()
        {
            return Color.Lerp(Color.yellow, EColor.Green.GetColor(), Mathf.Pow(fValue / (maxValue - minValue), 2));
        }

        private string Title(float curValue, float min, float max, string label)
        {
            // string percent = ;
            string p = (curValue / max).ToString("P");
            return curValue < 0 ? $"[{label}] Game Over" : $"[{label}] {p}";
        }
    }
}
