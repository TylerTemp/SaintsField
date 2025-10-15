using System;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.RichLabelExamples
{
    public class RichLabelExample: MonoBehaviour
    {
        [SepTitle("Rich Label", EColor.Green)]
        // [InfoBox("1")]
        // [InfoBox("2")]
        // [InfoBox("3")]
        // [InfoBox("4")]
        // [InfoBox("5")]
        [FieldRichLabel("<color=indigo><icon=star.png /></color><b><color=red>R</color><color=green>a</color><color=blue>i</color><color=yellow>i</color><color=cyan>n</color><color=magenta>b</color><color=pink>o</color><color=orange>w</color></b>: <color=violet><label /></color>")]
        public RichLabelExample _rainbow;

        [FieldRichLabel("$" + nameof(LabelCallback))]
        public bool _callbackToggle;
        private string LabelCallback() => _callbackToggle ? "<color=" + EColor.Green + "><icon=star.png /></color> <label/>" : "<icon=star-slash.png /> <label/>";

        [FieldRichLabel("$" + nameof(ArrayLabels))]
        public string[] arrayLabels;

        private string ArrayLabels(int index) => $"<color=pink>[{(char)('A' + index)}]";

        [Space]
        [FieldRichLabel("$" + nameof(_propertyLabel))]
        public string _propertyLabel;

        [Space]

        [FieldRichLabel("<color=aqua>██ EColor ██</color>")] public string colorAqua = "Aqua";
        [FieldRichLabel("<color=black>██ EColor ██</color>")] public string colorBlack = "Black";
        [FieldRichLabel("<color=blue>██ EColor ██</color>")] public string colorBlue = "Blue";
        [FieldRichLabel("<color=brown>██ EColor ██</color>")] public string colorBrown = "Brown";
        [FieldRichLabel("<color=cyan>██ EColor ██</color>")] public string colorCyan = "Cyan";
        [FieldRichLabel("<color=charcoalGray>██ EColor ██</color>")] public string colorCharcoalGray = "CharcoalGray";
        [FieldRichLabel("<color=darkBlue>██ EColor ██</color>")] public string colorDarkBlue = "DarkBlue";
        [FieldRichLabel("<color=fuchsia>██ EColor ██</color>")] public string colorFuchsia = "Fuchsia";
        [FieldRichLabel("<color=green>██ EColor ██</color>")] public string colorGreen = "Green";
        [FieldRichLabel("<color=gray>██ EColor ██</color>")] public string colorGray = "Gray";
        [FieldRichLabel("<color=grey>██ EColor ██</color>")] public string colorGrey = "Grey";
        [FieldRichLabel("<color=oceanicSlate>██ EColor ██</color>")] public string colorOceanicSlate = "OceanicSlate";
        [FieldRichLabel("<color=lightBlue>██ EColor ██</color>")] public string colorLightBlue = "LightBlue";
        [FieldRichLabel("<color=lime>██ EColor ██</color>")] public string colorLime = "Lime";
        [FieldRichLabel("<color=magenta>██ EColor ██</color>")] public string colorMagenta = "Magenta";
        [FieldRichLabel("<color=maroon>██ EColor ██</color>")] public string colorMaroon = "Maroon";
        [FieldRichLabel("<color=navy>██ EColor ██</color>")] public string colorNavy = "Navy";
        [FieldRichLabel("<color=olive>██ EColor ██</color>")] public string colorOlive = "Olive";
        [FieldRichLabel("<color=orange>██ EColor ██</color>")] public string colorOrange = "Orange";
        [FieldRichLabel("<color=purple>██ EColor ██</color>")] public string colorPurple = "Purple";
        [FieldRichLabel("<color=red>██ EColor ██</color>")] public string colorRed = "Red";
        [FieldRichLabel("<color=silver>██ EColor ██</color>")] public string colorSilver = "Silver";
        [FieldRichLabel("<color=teal>██ EColor ██</color>")] public string colorTeal = "Teal";
        [FieldRichLabel("<color=white>██ EColor ██</color>")] public string colorWhite = "White";
        [FieldRichLabel("<color=yellow>██ EColor ██</color>")] public string colorYellow = "Yellow";
        [FieldRichLabel("<color=clear>██ EColor ██</color>")] public string colorClear = "Clear";
        [FieldRichLabel("<color=pink>██ EColor ██</color>")] public string colorPink = "Pink";
        [FieldRichLabel("<color=indigo>██ EColor ██</color>")] public string colorIndigo = "Indigo";
        [FieldRichLabel("<color=violet>██ EColor ██</color>")] public string colorViolet = "Violet";

        [Serializable]
        private struct MyStruct
        {
            [FieldRichLabel("<color=green>HI!</color>")]
            public float LabelFloat;

            [Space]
            [Scene]
            public int Scene;
            [AboveButton("", "above")]
            [SortingLayer]
            public string SortingLayer;
            [FieldRichLabel(null)]
            [MinMaxSlider(0f, 10f, 0.1f)]
            public Vector2 Slider;

            public float DefaultFloat;
        }

        // this is a workaround
        [SerializeField]
        [FieldRichLabel("<color=green>Fix For Struct</color>")]
        // [FieldDrawerConfig]
        private MyStruct _myStructWorkAround;

        [SerializeField]
        private MyStruct _defaultDrawerForStruct;

        [FieldRichLabel("hi<b>!</b><color=red>some <color=\"green\"><b>[<color=yellow><icon='eye.png' /></color><label /></b>]</color>:su<color='yellow'> ff</color> ix</color> and long long long text")]
        public int richLabel;

        [FieldRichLabel("This is a long ride for people have nothing to")]
        public int richLabel2;

        [ReadOnly]
        [FieldRichLabel("This Is a Long Drive for Someone with Nothing to Think About")]
        public int richLabelDisabled;

        [FieldRichLabel("<color=lime>Get Rich!</color>")]
        public UnityEvent<int, GameObject> richEvent;

        [FieldRichLabel("<field=\">><color=yellow>{0}</color><<\"/> <index=\"[<color=blue>>></color>{0}<color=blue><<</color>]\"/>")]
        public string[] sindices;
    }
}
