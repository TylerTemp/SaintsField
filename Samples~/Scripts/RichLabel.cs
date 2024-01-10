using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class RichLabel: MonoBehaviour
    {
        // [InfoBox("1")]
        // [InfoBox("2")]
        // // [InfoBox("3")]
        // // [InfoBox("4")]
        // // [InfoBox("5")]
        // // [RichLabel("<color=indigo><icon=eye.png /></color><b><color=red>R</color><color=green>a</color><color=blue>i</color><color=yellow>i</color><color=cyan>n</color><color=magenta>b</color><color=pink>o</color><color=orange>w</color></b>: <color=violet><label /></color>")]
        // public int _rainbow;

        [SepTitle("test", EColor.Yellow)]
        [InfoBox("another1")]
        [InfoBox("another2")]
        public int another;

        // [RichLabel(nameof(LabelCallback), true)]
        // public bool _callbackToggle;
        // private string LabelCallback() => _callbackToggle ? "<color=" + EColor.Green + "><icon=eye.png /></color> <label/>" : "<icon=eye-slash.png /> <label/>";
        //
        // [RichLabel(nameof(ArrayLabels), true)]
        // public string[] arrayLabels;
        //
        // private string ArrayLabels(int index) => $"<color=pink>[{(char)('A' + index)}]";
        //
        // [Space]
        // [RichLabel(nameof(_propertyLabel), true)]
        // public string _propertyLabel;
        //
        // [Space]
        //
        // [RichLabel("<color=aqua>██ EColor.</color>Aqua")] public string colorAqua;
        // [RichLabel("<color=black>██ EColor.</color>Black")] public string colorBlack;
        // [RichLabel("<color=blue>██ EColor.</color>Blue")] public string colorBlue;
        // [RichLabel("<color=brown>██ EColor.</color>Brown")] public string colorBrown;
        // [RichLabel("<color=cyan>██ EColor.</color>Cyan")] public string colorCyan;
        // [RichLabel("<color=darkblue>██ EColor.</color>DarkBlue")] public string colorDarkBlue;
        // [RichLabel("<color=fuchsia>██ EColor.</color>Fuchsia")] public string colorFuchsia;
        // [RichLabel("<color=green>██ EColor.</color>Green")] public string colorGreen;
        // [RichLabel("<color=gray>██ EColor.</color>Gray")] public string colorGray;
        // [RichLabel("<color=grey>██ EColor.</color>Grey")] public string colorGrey;
        // [RichLabel("<color=lightblue>██ EColor.</color>LightBlue")] public string colorLightBlue;
        // [RichLabel("<color=lime>██ EColor.</color>Lime")] public string colorLime;
        // [RichLabel("<color=magenta>██ EColor.</color>Magenta")] public string colorMagenta;
        // [RichLabel("<color=maroon>██ EColor.</color>Maroon")] public string colorMaroon;
        // [RichLabel("<color=navy>██ EColor.</color>Navy")] public string colorNavy;
        // [RichLabel("<color=olive>██ EColor.</color>Olive")] public string colorOlive;
        // [RichLabel("<color=orange>██ EColor.</color>Orange")] public string colorOrange;
        // [RichLabel("<color=purple>██ EColor.</color>Purple")] public string colorPurple;
        // [RichLabel("<color=red>██ EColor.</color>Red")] public string colorRed;
        // [RichLabel("<color=silver>██ EColor.</color>Silver")] public string colorSilver;
        // [RichLabel("<color=teal>██ EColor.</color>Teal")] public string colorTeal;
        // [RichLabel("<color=white>██ EColor.</color>White")] public string colorWhite;
        // [RichLabel("<color=yellow>██ EColor.</color>Yellow")] public string colorYellow;
        // [RichLabel("<color=clear>██ EColor.</color>Clear")] public string colorClear;
        // [RichLabel("<color=pink>██ EColor.</color>Pink")] public string colorPink;
        // [RichLabel("<color=indigo>██ EColor.</color>Indigo")] public string colorIndigo;
        // [RichLabel("<color=violet>██ EColor.</color>Violet")] public string colorViolet;
        //
        // [Serializable]
        // private struct MyStruct
        // {
        //     [RichLabel("<color=green>HI!</color>")]
        //     public float LabelFloat;
        //
        //     [Space]
        //     [Scene]
        //     public int Scene;
        //     [AboveButton("", "above")]
        //     [SortingLayer]
        //     public string SortingLayer;
        //     // [RichLabel(null)]
        //     [MinMaxSlider(0f, 10f, 0.1f)]
        //     public Vector2 Slider;
        //
        //     public float DefaultFloat;
        // }
        //
        // // this is a workaround
        // [SerializeField]
        // [RichLabel("<color=green>Fix For Struct</color>")]
        // // [FieldDrawerConfig]
        // private MyStruct _myStructWorkAround;
        //
        // [SerializeField]
        // private MyStruct _defaultDrawerForStruct;
        //
        // [RichLabel("hi<b>!</b><color=red>some <color=\"green\"><b>[<color=yellow><icon='eye.png' /></color><label /></b>]</color>:su<color='yellow'> ff</color> ix</color> and long long long text")]
        // public int richLabel;
    }
}
