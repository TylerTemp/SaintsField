// using MarkupAttributes;

using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class CompatibilityNaAndDefault : MonoBehaviour
    {
        [RichLabel("<color=green>+Native</color>"), Range(0, 5)]
        public float nativeRange;
        // this wont work. Please put `SaintsField` before other drawers
        [Range(0, 5), RichLabel("<color=green>+Native</color>")]
        public float nativeRangeHandled;

#if SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
        // this works
        [RichLabel("<color=green>+NA</color>"), NaughtyAttributes.Label(" "), NaughtyAttributes.CurveRange(0, 0, 1, 1, NaughtyAttributes.EColor.Green)]
        public AnimationCurve naCurve;

        // this wont work too. Please put `SaintsField` before other drawers
        [NaughtyAttributes.CurveRange(0, 0, 1, 1, NaughtyAttributes.EColor.Green), RichLabel("<color=green>+NA</color>")]
        public AnimationCurve naCurveHandled;

        [RichLabel("<color=green>+NA</color>"), NaughtyAttributes.InputAxis, NaughtyAttributes.Label(" ")]
        public string inputAxis;

        [RichLabel("<color=green>+NA</color>"), NaughtyAttributes.ProgressBar(100), NaughtyAttributes.Label(" ")]
        public int naProgressBar = 30;

        // here is the native one
        [NaughtyAttributes.ProgressBar(100)]
        public int naNativeProgressBar = 30;

        // well, SaintsField has more function than NA's ProgressBar
        [ProgressBar(100), RichLabel("<color=green>+SF</color>")]
        public int sfProgressBar;

        [NaughtyAttributes.BoxGroup("Integers")]
        public int firstInt;

        public int sepHere;

        [NaughtyAttributes.BoxGroup("Integers")]
        public int secondInt;

        // https://github.com/dbrizov/NaughtyAttributes/issues/302

        [Serializable]
        public struct Nest2
        {
            [SaintsField.Required("SaintsField.Required: field is required")] public GameObject saints;
            [NaughtyAttributes.Required("NaughtyAttributes.Required: field is required")] public GameObject na;
        }

        [Serializable]
        public struct Nest1
        {
            public Nest2 nest2;
        }

        public Nest1 nest;

#endif

#if !SAINTSFIELD_SAMPLE_DISABLE_UNSAINTLY_EDITOR && !SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
        [Playa.ShowInInspector] public const float PI = 3.14f;
#endif
        //
        // [Tooltip("TT1")]
        // public int tt1;
        //
        // [RichLabel("Rich"), Tooltip("TT2"), MinMaxSlider(0, 10)] public Vector2 tt2;
        //
        // // [TabScope("Tab Scope", "Left|Right", box: true)]
        // // [Tab("./Left")]
        // // [MinValue(10), AboveRichLabel("Test Cap")]
        // // public int one;
        // //
        // // [Tab("../Right")]
        // // [MinMaxSlider(0, nameof(one)), RichLabel("MinMaxSlider")]
        // // public Vector2 two;
        // // public int three;
        //
        // // [Box("Group")]
        // // public int one;
        // // [TitleGroup("Group/Nested Group 1")]
        // // public int two;
        // // public int three;
        // // [TitleGroup("Group/Nested Group 2")]
        // // public int four;
        // // public int five;
    }
}
