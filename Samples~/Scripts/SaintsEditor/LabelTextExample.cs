using System;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LabelTextExample : SaintsMonoBehaviour
    {
        [SepTitle("Rich Label", EColor.Green)]
        // [InfoBox("1")]
        // [InfoBox("2")]
        // [InfoBox("3")]
        // [InfoBox("4")]
        // [InfoBox("5")]
        [LabelText("<color=indigo><icon=star.png /></color><b><color=red>R</color><color=green>a</color><color=blue>i</color><color=yellow>i</color><color=cyan>n</color><color=magenta>b</color><color=pink>o</color><color=orange>w</color></b>: <color=violet><label /></color>")]
        // [LabelText("<color=indigo><icon=star.png />")]
        public string _rainbow;

        [LabelText("$" + nameof(LabelCallback))]
        public bool _callbackToggle;
        private string LabelCallback() => _callbackToggle
            ? "<color=" + EColor.Green + "><icon=star.png /></color> <label/>"
            : "<icon=star-slash.png /> <label/>";

        [Space]
        [LabelText("$" + nameof(_propertyLabel))]
        public string _propertyLabel;

        [Space]

        [LabelText("<color=aqua>██ EColor ██</color>")] public string colorAqua = "Aqua";
        [LabelText("<color=black>██ EColor ██</color>")] public string colorBlack = "Black";
        [LabelText("<color=blue>██ EColor ██</color>")] public string colorBlue = "Blue";
        [LabelText("<color=brown>██ EColor ██</color>")] public string colorBrown = "Brown";
        [LabelText("<color=cyan>██ EColor ██</color>")] public string colorCyan = "Cyan";
        [LabelText("<color=charcoalGray>██ EColor ██</color>")] public string colorCharcoalGray = "CharcoalGray";
        [LabelText("<color=darkBlue>██ EColor ██</color>")] public string colorDarkBlue = "DarkBlue";
        [LabelText("<color=fuchsia>██ EColor ██</color>")] public string colorFuchsia = "Fuchsia";
        [LabelText("<color=green>██ EColor ██</color>")] public string colorGreen = "Green";
        [LabelText("<color=gray>██ EColor ██</color>")] public string colorGray = "Gray";
        [LabelText("<color=grey>██ EColor ██</color>")] public string colorGrey = "Grey";
        [LabelText("<color=oceanicSlate>██ EColor ██</color>")] public string colorOceanicSlate = "OceanicSlate";
        [LabelText("<color=lightBlue>██ EColor ██</color>")] public string colorLightBlue = "LightBlue";
        [LabelText("<color=lime>██ EColor ██</color>")] public string colorLime = "Lime";
        [LabelText("<color=magenta>██ EColor ██</color>")] public string colorMagenta = "Magenta";
        [LabelText("<color=maroon>██ EColor ██</color>")] public string colorMaroon = "Maroon";
        [LabelText("<color=navy>██ EColor ██</color>")] public string colorNavy = "Navy";
        [LabelText("<color=olive>██ EColor ██</color>")] public string colorOlive = "Olive";
        [LabelText("<color=orange>██ EColor ██</color>")] public string colorOrange = "Orange";
        [LabelText("<color=purple>██ EColor ██</color>")] public string colorPurple = "Purple";
        [LabelText("<color=red>██ EColor ██</color>")] public string colorRed = "Red";
        [LabelText("<color=silver>██ EColor ██</color>")] public string colorSilver = "Silver";
        [LabelText("<color=teal>██ EColor ██</color>")] public string colorTeal = "Teal";
        [LabelText("<color=white>██ EColor ██</color>")] public string colorWhite = "White";
        [LabelText("<color=yellow>██ EColor ██</color>")] public string colorYellow = "Yellow";
        [LabelText("<color=clear>██ EColor ██</color>")] public string colorClear = "Clear";
        [LabelText("<color=pink>██ EColor ██</color>")] public string colorPink = "Pink";
        [LabelText("<color=indigo>██ EColor ██</color>")] public string colorIndigo = "Indigo";
        [LabelText("<color=violet>██ EColor ██</color>")] public string colorViolet = "Violet";

        [Serializable]
        private struct MyStruct
        {
            [LabelText("<color=green>HI!</color>")]
            public float LabelFloat;

            [Space]
            [Scene]
            public int Scene;
            [AboveButton("", "above")]
            [SortingLayer]
            public string SortingLayer;
            [NoLabel]
            [MinMaxSlider(0f, 10f, 0.1f)]
            public Vector2 Slider;

            public float DefaultFloat;
        }

        [SerializeField]
        [LabelText("<color=green>Fix For Struct</color>")]
        private MyStruct _myStructWorkAround;

        [SerializeField]
        private MyStruct _defaultDrawerForStruct;

        [LabelText("hi<b>!</b><color=red>some <color=\"green\"><b>[<color=yellow><icon='star.png' /></color><label /></b>]</color>:su<color='yellow'> ff</color> ix</color> and long long long text")]
        public int richLabel;

        [LabelText("This is a long ride for people have nothing to")]
        public int richLabel2;

        [ReadOnly]
        [LabelText("This Is a Long Drive for Someone with Nothing to Think About")]
        public int richLabelDisabled;

        [LabelText("<color=lime>Get Rich!</color>")]
        public UnityEvent<int, GameObject> richEvent;

        [LabelText("array <label/>")]
        public string[] sindices;
    }
}
