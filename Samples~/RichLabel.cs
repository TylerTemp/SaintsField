using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class RichLabel: MonoBehaviour
    {
        [RichLabel("<color=indigo><icon=eye.png /></color><b><color=red>R</color><color=green>a</color><color=blue>i</color><color=yellow>i</color><color=cyan>n</color><color=magenta>b</color><color=pink>o</color><color=orange>w</color></b>: <color=violet><label /></color>")]
        public int _rainbow;

        [RichLabel(nameof(LabelCallback), true)]
        public bool _callbackToggle;
        private string LabelCallback() => _callbackToggle ? "<color=green><icon=eye.png /></color> <label/>" : "<icon=eye-slash.png /> <label/>";

        [RichLabel(nameof(ArrayLabels), true)]
        public string[] arrayLabels;

        private string ArrayLabels(int index) => $"<color=pink>[{(char)('A' + index)}]";

        [Space]
        [RichLabel(nameof(_propertyLabel), true)]
        public string _propertyLabel;

        [RichLabel("<color=clear>██</color> Clear")] public string colorClear;
        [RichLabel("<color=white>██</color> White")] public string colorWhite;
        [RichLabel("<color=black>██</color> Black")] public string colorBlack;
        [RichLabel("<color=gray>██</color> Gray")] public string colorGray;
        [RichLabel("<color=red>██</color> Red")] public string colorRed;
        [RichLabel("<color=pink>██</color> Pink")] public string colorPink;
        [RichLabel("<color=orange>██</color> Orange")] public string colorOrange;
        [RichLabel("<color=yellow>██</color> Yellow")] public string colorYellow;
        [RichLabel("<color=green>██</color> Green")] public string colorGreen;
        [RichLabel("<color=blue>██</color> Blue")] public string colorBlue;
        [RichLabel("<color=indigo>██</color> Indigo")] public string colorIndigo;
        [RichLabel("<color=violet>██</color> Violet")] public string colorViolet;
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
        // [FieldDrawerConfig]
        private MyStruct _myStructWorkAround;

        [SerializeField]
        private MyStruct _defaultDrawerForStruct;

        [RichLabel("hi<b>!</b><color=red>some <color=\"green\"><b>[<color=yellow><icon='eye.png' /></color><label /></b>]</color>:su<color='yellow'> ff</color> ix</color> and long long long text")]
        public int richLabel;
    }
}
