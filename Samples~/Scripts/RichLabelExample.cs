using System;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts
{
    public class RichLabelExample: MonoBehaviour
    {
        [SepTitle("Rich Label", EColor.Green)]
        // [InfoBox("1")]
        // [InfoBox("2")]
        // [InfoBox("3")]
        // [InfoBox("4")]
        // [InfoBox("5")]
        [RichLabel("<color=indigo><icon=star.png /></color><b><color=red>R</color><color=green>a</color><color=blue>i</color><color=yellow>i</color><color=cyan>n</color><color=magenta>b</color><color=pink>o</color><color=orange>w</color></b>: <color=violet><label /></color>")]
        public RichLabelExample _rainbow;

        [RichLabel("$" + nameof(LabelCallback))]
        public bool _callbackToggle;
        private string LabelCallback() => _callbackToggle ? "<color=" + EColor.Green + "><icon=star.png /></color> <label/>" : "<icon=star-slash.png /> <label/>";

        [RichLabel("$" + nameof(ArrayLabels))]
        public string[] arrayLabels;

        private string ArrayLabels(int index) => $"<color=pink>[{(char)('A' + index)}]";

        [Space]
        [RichLabel("$" + nameof(_propertyLabel))]
        public string _propertyLabel;

        [Space]

        [RichLabel("<color=aqua>██ EColor ██</color>")] public string colorAqua = "Aqua";
        [RichLabel("<color=black>██ EColor ██</color>")] public string colorBlack = "Black";
        [RichLabel("<color=blue>██ EColor ██</color>")] public string colorBlue = "Blue";
        [RichLabel("<color=brown>██ EColor ██</color>")] public string colorBrown = "Brown";
        [RichLabel("<color=cyan>██ EColor ██</color>")] public string colorCyan = "Cyan";
        [RichLabel("<color=charcoalGray>██ EColor ██</color>")] public string colorCharcoalGray = "CharcoalGray";
        [RichLabel("<color=darkBlue>██ EColor ██</color>")] public string colorDarkBlue = "DarkBlue";
        [RichLabel("<color=fuchsia>██ EColor ██</color>")] public string colorFuchsia = "Fuchsia";
        [RichLabel("<color=green>██ EColor ██</color>")] public string colorGreen = "Green";
        [RichLabel("<color=gray>██ EColor ██</color>")] public string colorGray = "Gray";
        [RichLabel("<color=grey>██ EColor ██</color>")] public string colorGrey = "Grey";
        [RichLabel("<color=oceanicSlate>██ EColor ██</color>")] public string colorOceanicSlate = "OceanicSlate";
        [RichLabel("<color=lightBlue>██ EColor ██</color>")] public string colorLightBlue = "LightBlue";
        [RichLabel("<color=lime>██ EColor ██</color>")] public string colorLime = "Lime";
        [RichLabel("<color=magenta>██ EColor ██</color>")] public string colorMagenta = "Magenta";
        [RichLabel("<color=maroon>██ EColor ██</color>")] public string colorMaroon = "Maroon";
        [RichLabel("<color=navy>██ EColor ██</color>")] public string colorNavy = "Navy";
        [RichLabel("<color=olive>██ EColor ██</color>")] public string colorOlive = "Olive";
        [RichLabel("<color=orange>██ EColor ██</color>")] public string colorOrange = "Orange";
        [RichLabel("<color=purple>██ EColor ██</color>")] public string colorPurple = "Purple";
        [RichLabel("<color=red>██ EColor ██</color>")] public string colorRed = "Red";
        [RichLabel("<color=silver>██ EColor ██</color>")] public string colorSilver = "Silver";
        [RichLabel("<color=teal>██ EColor ██</color>")] public string colorTeal = "Teal";
        [RichLabel("<color=white>██ EColor ██</color>")] public string colorWhite = "White";
        [RichLabel("<color=yellow>██ EColor ██</color>")] public string colorYellow = "Yellow";
        [RichLabel("<color=clear>██ EColor ██</color>")] public string colorClear = "Clear";
        [RichLabel("<color=pink>██ EColor ██</color>")] public string colorPink = "Pink";
        [RichLabel("<color=indigo>██ EColor ██</color>")] public string colorIndigo = "Indigo";
        [RichLabel("<color=violet>██ EColor ██</color>")] public string colorViolet = "Violet";

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
            [RichLabel(null)]
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

        [RichLabel("This is a long ride for people have nothing to")]
        public int richLabel2;

        [ReadOnly]
        [RichLabel("This Is a Long Drive for Someone with Nothing to Think About")]
        public int richLabelDisabled;

        [RichLabel("<color=lime>Get Rich!</color>")]
        public UnityEvent<int, GameObject> richEvent;
    }
}
