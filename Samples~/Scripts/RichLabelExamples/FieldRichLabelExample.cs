using System;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.RichLabelExamples
{
    public class FieldRichLabelExample: MonoBehaviour
    {
        [SepTitle("Rich Label", EColor.Green)]
        // [InfoBox("1")]
        // [InfoBox("2")]
        // [InfoBox("3")]
        // [InfoBox("4")]
        // [InfoBox("5")]
        [FieldLabelText("<color=indigo><icon=star.png /></color><b><color=red>R</color><color=green>a</color><color=blue>i</color><color=yellow>i</color><color=cyan>n</color><color=magenta>b</color><color=pink>o</color><color=orange>w</color></b>: <color=violet><label /></color>")]
        public FieldRichLabelExample _rainbow;

        [FieldLabelText("$" + nameof(LabelCallback))]
        public bool _callbackToggle;
        private string LabelCallback() => _callbackToggle ? "<color=" + EColor.Green + "><icon=star.png /></color> <label/>" : "<icon=star-slash.png /> <label/>";

        [FieldLabelText("$" + nameof(ArrayLabels))]
        public string[] arrayLabels;

        private string ArrayLabels(int index) => $"<color=pink>[{(char)('A' + index)}]";

        [Space]
        [FieldLabelText("$" + nameof(_propertyLabel))]
        public string _propertyLabel;

        [Space]

        [FieldLabelText("<color=aqua>██ EColor ██</color>")] public string colorAqua = "Aqua";
        [FieldLabelText("<color=black>██ EColor ██</color>")] public string colorBlack = "Black";
        [FieldLabelText("<color=blue>██ EColor ██</color>")] public string colorBlue = "Blue";
        [FieldLabelText("<color=brown>██ EColor ██</color>")] public string colorBrown = "Brown";
        [FieldLabelText("<color=cyan>██ EColor ██</color>")] public string colorCyan = "Cyan";
        [FieldLabelText("<color=charcoalGray>██ EColor ██</color>")] public string colorCharcoalGray = "CharcoalGray";
        [FieldLabelText("<color=darkBlue>██ EColor ██</color>")] public string colorDarkBlue = "DarkBlue";
        [FieldLabelText("<color=fuchsia>██ EColor ██</color>")] public string colorFuchsia = "Fuchsia";
        [FieldLabelText("<color=green>██ EColor ██</color>")] public string colorGreen = "Green";
        [FieldLabelText("<color=gray>██ EColor ██</color>")] public string colorGray = "Gray";
        [FieldLabelText("<color=grey>██ EColor ██</color>")] public string colorGrey = "Grey";
        [FieldLabelText("<color=oceanicSlate>██ EColor ██</color>")] public string colorOceanicSlate = "OceanicSlate";
        [FieldLabelText("<color=lightBlue>██ EColor ██</color>")] public string colorLightBlue = "LightBlue";
        [FieldLabelText("<color=lime>██ EColor ██</color>")] public string colorLime = "Lime";
        [FieldLabelText("<color=magenta>██ EColor ██</color>")] public string colorMagenta = "Magenta";
        [FieldLabelText("<color=maroon>██ EColor ██</color>")] public string colorMaroon = "Maroon";
        [FieldLabelText("<color=navy>██ EColor ██</color>")] public string colorNavy = "Navy";
        [FieldLabelText("<color=olive>██ EColor ██</color>")] public string colorOlive = "Olive";
        [FieldLabelText("<color=orange>██ EColor ██</color>")] public string colorOrange = "Orange";
        [FieldLabelText("<color=purple>██ EColor ██</color>")] public string colorPurple = "Purple";
        [FieldLabelText("<color=red>██ EColor ██</color>")] public string colorRed = "Red";
        [FieldLabelText("<color=silver>██ EColor ██</color>")] public string colorSilver = "Silver";
        [FieldLabelText("<color=teal>██ EColor ██</color>")] public string colorTeal = "Teal";
        [FieldLabelText("<color=white>██ EColor ██</color>")] public string colorWhite = "White";
        [FieldLabelText("<color=yellow>██ EColor ██</color>")] public string colorYellow = "Yellow";
        [FieldLabelText("<color=clear>██ EColor ██</color>")] public string colorClear = "Clear";
        [FieldLabelText("<color=pink>██ EColor ██</color>")] public string colorPink = "Pink";
        [FieldLabelText("<color=indigo>██ EColor ██</color>")] public string colorIndigo = "Indigo";
        [FieldLabelText("<color=violet>██ EColor ██</color>")] public string colorViolet = "Violet";

        [Serializable]
        private struct MyStruct
        {
            [FieldLabelText("<color=green>HI!</color>")]
            public float LabelFloat;

            [Space]
            [Scene]
            public int Scene;
            [AboveButton("", "above")]
            [SortingLayer]
            public string SortingLayer;
            [FieldLabelText(null)]
            [MinMaxSlider(0f, 10f, 0.1f)]
            public Vector2 Slider;

            public float DefaultFloat;
        }

        // this is a workaround
        [SerializeField]
        [FieldLabelText("<color=green>Fix For Struct</color>")]
        // [FieldDrawerConfig]
        private MyStruct _myStructWorkAround;

        [SerializeField]
        private MyStruct _defaultDrawerForStruct;

        [FieldLabelText("hi<b>!</b><color=red>some <color=\"green\"><b>[<color=yellow><icon='eye.png' /></color><label /></b>]</color>:su<color='yellow'> ff</color> ix</color> and long long long text")]
        public int richLabel;

        [FieldLabelText("This is a long ride for people have nothing to")]
        public int richLabel2;

        [ReadOnly]
        [FieldLabelText("This Is a Long Drive for Someone with Nothing to Think About")]
        public int richLabelDisabled;

        [FieldLabelText("<color=lime>Get Rich!</color>")]
        public UnityEvent<int, GameObject> richEvent;

        [FieldLabelText("<field=\">><color=yellow>{0}</color><<\"/> <index=\"[<color=blue>>></color>{0}<color=blue><<</color>]\"/>")]
        public string[] sindices;
    }
}
