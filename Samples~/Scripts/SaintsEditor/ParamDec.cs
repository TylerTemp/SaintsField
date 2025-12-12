using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace Saintsfield.Samples.Scripts.SaintsEditor
{
    public class ParamDec : SaintsMonoBehaviour
    {
        [Separator("Rate")]
        [ShowInInspector]
        [Button]
        [Rate(1, 5)]
        private int ShowRate([Rate(0, 5)] int rate) => rate;

        [Separator("PropRange")]
        [ShowInInspector]
        [Button]
        [PropRange(0, 100)]
        private int ShowPropR([PropRange(0, 100)] int p) => p;

        [Separator(5)]
        [Separator("MinMaxSlider")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [MinMaxSlider(-10, 10)]
        private Vector2Int MinMaxV2([MinMaxSlider(-10, 10)] Vector2Int minMax) => minMax;

        [Separator(5)]
        [Separator("ProgressBar")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [ProgressBar(0, 10)]
        private int ProgressBar([ProgressBar(0, 10)] int hp) => hp;

        [Separator(5)]
        [Separator("Scene")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [Scene]
        private (int i, string s) ButtonParamScene([Scene] int sceneI, [Scene] string sceneS)
        {
            return (sceneI, sceneS);
        }

        [Separator(5)]
        [Separator("Tag")]
        [Separator(5)]
        [Tag]
        [ShowInInspector]
        [Button]
        private string Tag([Tag] string myTag) => myTag;

        [Separator(5)]
        [Separator("InputAxis")]
        [Separator(5)]
        [InputAxis]
        [ShowInInspector]
        [Button]
        private string ShowInputAxis([InputAxis] string myInput) => myInput;

        [Separator(5)]
        [Separator("InputAxis")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [ShaderParam]
        private string ShowShaderParam([ShaderParam] string shaderS) => shaderS;

        [Separator(5)]
        [Separator("DateTime")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [DateTime]
        private long ShowDatetime([DateTime] long dt) => dt;

        [Separator(5)]
        [Separator("TimeSpan")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [TimeSpan]
        private long ShowTimeSpan([TimeSpan] long ts) => ts;

        [Separator(5)]
        [Separator("TimeSpan")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [Guid]
        private string ShowGuid([Guid] string guidString) => guidString;

        [Separator(5)]
        [Separator("AnimatorParam")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [AnimatorParam]
        private int ShowAnimatorParam([AnimatorParam] string animName) => Animator.StringToHash(animName);

        [Separator(5)]
        [Separator("AnimatorParam")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [AnimatorState]
        private string ShowAnimatorState([AnimatorState] string animName) => animName;

        [Separator(5)]
        [Separator("CurveRange")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [CurveRange(EColor.Aquamarine)]
        private AnimationCurve ShowCurveRange([CurveRange(EColor.YellowNice)] AnimationCurve animCurve) => animCurve;

        [Separator(5)]
        [Separator("ValueButtons")]
        [Separator(5)]
        [ShowInInspector]
        [Button]
        [ValueButtons(nameof(ValueButtonsResultProvider))]
        private int ShowValueButton([ValueButtons(nameof(ValueButtonsOptionProvider))] int opt) => opt;

        private AdvancedDropdownList<int> ValueButtonsOptionProvider() => new AdvancedDropdownList<int>
        {
            { "<icon=lightMeter/greenLight/>", 2 },
            { "<icon=lightMeter/redLight/>", 4 },
        };

        private AdvancedDropdownList<int> ValueButtonsResultProvider() => new AdvancedDropdownList<int>
        {
            { "<icon=toggle on focus@2x/>", 2 },
            { "<icon=toggle focus@2x/>", 4 },
        };
    }
}
