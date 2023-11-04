using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class RichLabel: MonoBehaviour
    {
        [RichLabel("<color=indigo><icon=eye.png /></color><b><color=red>R</color><color=green>a</color><color=blue>i</color><color=yellow>i</color><color=cyan>n</color><color=magenta>b</color><color=pink>o</color><color=orange>w</color></b>: <color=violet><label /></color>")]
        public string _rainbow;

        [RichLabel(nameof(LabelCallback), true)]
        public bool _callbackToggle;
        private string LabelCallback() => _callbackToggle ? "<color=green><icon=eye.png /></color> <label/>" : "<icon=eye-slash.png /> <label/>";

        [Space]
        [RichLabel(nameof(_propertyLabel), true)]
        public string _propertyLabel;

        [Serializable]
        private struct MyStruct
        {
            [RichLabel("<color=green>HI!</color>")]
            public float LabelFloat;

            [Space]
            [Scene]
            public int Scene;
            [AboveButton("", "above")]
            [SortingLayer]
            public string SortingLayer;
            // [RichLabel(null)]
            [MinMaxSlider(0f, 10f, 0.1f)]
            public Vector2 Slider;

            public float DefaultFloat;
        }

        // this is a workaround
        [SerializeField]
        [RichLabel("<color=green>Fix For Struct</color>")]
        [FieldDrawerConfig]
        private MyStruct _myStructWorkAround;


        [SerializeField]
        [Space(100)]
        [SepTitle(null, EColor.Black)]
        [RichLabel("<color=green>Weird: <label /></color>")]
        private MyStruct _myStructBadView;

        [RichLabel("hi<b>!</b><color=red>some <color=\"green\"><b>[<color=yellow><icon='eye.png' /></color><label /></b>]</color>:su<color='yellow'> ff</color> ix</color> and long long long text")]
        public string richLabel;

    }
}
