using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class RichLabel: MonoBehaviour
    {
        // [RichLabel("prefix:<color=red>some <color=\"green\"><b>[<color=yellow><icon='eye.png' /></color><label /></b>]</color>:su<color='yellow'> ff</color> ix</color> and long long long text")]
        // public string richLabel;

        [Serializable]
        private struct MyStruct
        {
            [Scene]
            public int Scene;
            [AboveButton("", "above")]
            [SortingLayer]
            public string SortingLayer;
            // [RichLabel(null)]
            [MinMaxSlider(0f, 10f, 0.1f)]
            public Vector2 Slider;

            [RichLabel("<color=green>HI!</color>")]
            public float LabelFloat;

            public float DefaultFloat;
        }

        [SerializeField]
        [RichLabel("<color=green>Weird: <label /></color>")]
        private MyStruct _myStructBadView;

        // this is a workaround
        [SerializeField]
        [RichLabel("<color=green>OK: <label /></color>")]
        [FieldDrawerConfig(FieldDrawerConfigAttribute.FieldDrawType.FullWidthOverlay)]
        private MyStruct _myStructWorkAround;

        // [SerializeField]
        // [RichLabel("<color=green>Bug: <label /></color>")]
        // [FieldDrawerConfig(FieldDrawerConfigAttribute.FieldDrawType.FullWidthNewLine)]
        // private MyStruct _example;
        //
        // [SerializeField] private int _int1;
        // [SerializeField] private int _int2;

        // [SerializeField]
        // private MyStruct _pure;
    }
}
