using System;
using System.Reflection;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue29 : MonoBehaviour
    {
        [Serializable]
        public enum Toggle
        {
            Off,
            On,
        }

        [Serializable]
        public struct SimpleExampleStruct
        {
            public Toggle toggle;
            [FieldShowIf(nameof(toggle), Toggle.On)] public float value;
        }

        public SimpleExampleStruct example;

        [Serializable]
        public struct MyStruct
        {
            [OnValueChanged(nameof(Log))]
            public Toggle toggle;

            [FieldBelowText(nameof(toggle), true)]
            [FieldAboveText(nameof(toggle), true)]
            [FieldLabelText(nameof(toggle), true)]
            [OverlayText(nameof(toggle), true)]
            [EndText(nameof(toggle), true)]
            [FieldInfoBox(nameof(toggle), true)]
            [AboveButton(nameof(Log), nameof(toggle), true)]
            [BelowButton(nameof(Log), nameof(toggle), true)]
            [PostFieldButton(nameof(Log), nameof(toggle), true)]
            public float displayTypeDec;

            [SepTitle(EColor.Aqua)]
            [AboveImage(nameof(spriteRenderer), 10)]
            [BelowImage(nameof(spriteRenderer), 10)]
            public SpriteRenderer spriteRenderer;
            [SpriteToggle(nameof(spriteRenderer))] public Sprite sprite1;
            [SpriteToggle(nameof(spriteRenderer))] public Sprite sprite2;
            [ColorToggle(nameof(spriteRenderer))] public Color color1;
            [ColorToggle(nameof(spriteRenderer))] public Color color2;

            [SepTitle(EColor.Aqua)]
            public Renderer matRenderer;
            [MaterialToggle(nameof(matRenderer))] public Material mat1;
            [MaterialToggle(nameof(matRenderer))] public Material mat2;

            [SepTitle(EColor.Aqua)]
            [DisableIf(nameof(toggle), Toggle.On)]
            public float disableIf;
            [EnableIf(nameof(toggle), Toggle.On)]
            public float enableIf;
            [FieldShowIf(nameof(toggle), Toggle.On)]
            public float showIf;
            [FieldHideIf(nameof(toggle), Toggle.On)]
            public float hideIf;

            [SepTitle(EColor.Aqua)]
            [Required] public GameObject required;

            [SepTitle(EColor.Aqua)]
            public int startRange;
            public int endRange;
            [PropRange(nameof(startRange), nameof(endRange))] public int propRangeInt;
            [ProgressBar(nameof(startRange), nameof(endRange))] public int progressBar;
            [MinMaxSlider(nameof(startRange), nameof(endRange))] public Vector2Int minMaxSlider;
            [MinValue(nameof(startRange))] public int minValue;
            [MaxValue(nameof(endRange))] public int maxValue;

            [SepTitle(EColor.Aqua)]
            public Animator animator;
            [AnimatorParam(nameof(animator))] public int animatorParaHash;
            [AnimatorState(nameof(animator))] public AnimatorStateBase animatorState;

            private void Log(object v, int index = -1) => Debug.Log($"{v}@{index}");
        }

        public MyStruct[] myStructs;
    }
}
