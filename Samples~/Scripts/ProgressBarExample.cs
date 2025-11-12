using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ProgressBarExample: SaintsMonoBehaviour
    {
        [ProgressBar(0, 10, -1, color: EColor.Brown)][LabelText("<icon=star.png /><label/>")]
        public int myHp;

        [ShowInInspector, ProgressBar(0, 10), LabelText("<color=green><label/>")]
        public int ShowMyHp
        {
            get => myHp;
            set => myHp = value;
        }

        [ProgressBar(0, 100f, step: 0.05f, color: EColor.Blue)]
        public float myMp;

        [Space]
        public int minValue;
        public int maxValue;

        [ProgressBar(nameof(minValue)
                , nameof(maxValue)
                , step: 0.05f
                , backgroundColorCallback: nameof(BackgroundColor)
                , colorCallback: nameof(FillColor)
                , titleCallback: nameof(Title)
            ),
        ]
        [FieldLabelText(null)]
        public float fValue;

        private EColor BackgroundColor() => fValue <= 0? EColor.Brown: EColor.CharcoalGray;

        private Color FillColor()
        {
            return Color.Lerp(Color.yellow, EColor.Green.GetColor(),
                Mathf.Pow(Mathf.InverseLerp(minValue, maxValue, fValue), 2));
        }

        private string Title(float curValue, float min, float max, string label) => curValue < 0 ? $"[{label}] Game Over: {curValue}" : $"[{label}] {curValue / max:P}";

        [ProgressBar(0, nameof(maxStamina), colorCallback: nameof(StaminaColor), titleCallback: nameof(StaminaTitle))]
        [FieldLabelText(null)]
        public int stamina = 50;
        public int highStamina = 150;
        public int maxStamina = 200;

        private Color StaminaColor()
        {
            if (stamina < highStamina)
            {
                return Color.red;
            }
            return Color.Lerp(Color.yellow, Color.green, Mathf.InverseLerp(highStamina, maxStamina, stamina));
        }

        private string StaminaTitle(float curValue, float min, float max, string label) => $"[{label}] {curValue / max:P}";

        [ReadOnly]
        [ProgressBar(10)][FieldLabelText("<icon=star.png /><label/>")]
        public int myHpDisabled;
    }
}
