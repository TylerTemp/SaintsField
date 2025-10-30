using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class HorizontalLayout : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string ss;
            public int si;
        }

        [ProgressBar(min: 0, max: 100, color: EColor.Red)] public int hp;
        [ProgressBar(min: 0, max: 100, color: EColor.Blue)] public int mp;

        [LayoutStart("Equipment", ELayout.TitleBox | ELayout.Vertical)]
        [LayoutStart("./Head", ELayout.TitleBox)]
        public string helmet;

        [LayoutStart("../Upper Body", ELayout.TitleBox)]

        [InfoBox("Note：left hand can be empty, but not right hand")]

        [LayoutStart("./Horizontal", ELayout.Horizontal)]

        [LayoutStart("./Left Hand", ELayout.TitleBox)]
        [BelowImage]
        public Sprite g11;
        [FieldLabelText("$" + nameof(g22))]
        public string[] g23;
        public bool toggle;

        [LayoutStart("../Right Hand", ELayout.TitleBox)]
        [Expandable]
        public Scriptable g22;

        [LayoutEnd]

        [InfoBox("Buttons!")]
        [LayoutStart("Buttons", ELayout.Horizontal)]
        [Button]
        public void B1(string strV, bool bv, Vector2 v2)
        {

        }

        [Button]
        public void B2(MyStruct structV, Vector4 v4, Bounds bounds, Rect rectValue)
        {

        }

        [LayoutStart("V", ELayout.TitleBox)]
        [Expandable] public Scriptable soV;
        [AnimatorState] public AnimatorStateBase animBaseV;

        [LayoutStart("Horizontal", ELayout.TitleBox | ELayout.Horizontal)]
        [Expandable] public Scriptable so;
        [AnimatorState] public AnimatorStateBase animBase;
        // [LayoutStart("Tab", ELayout.TitleBox)]
        // // public string tab;
        //
        // // [LayoutStart("./1", ELayout.TitleBox)]
        // // public string tab1Sub1;
        // public string tab1Sub2;
        // [LayoutTerminateHere]
        // public string tab1Sub3;
        //
        // [Button]
        // public void AFunction() {}
        // [Button]
        // public void BFunction() {}
    }
}
