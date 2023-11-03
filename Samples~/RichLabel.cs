using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class RichLabel: MonoBehaviour
    {
        [RichLabel("prefix:<color=red>some <color=\"green\"><b>[<color=yellow><icon='eye.png' /></color><label /></b>]</color>:su<color='yellow'> ff</color> ix</color> and long long long text")]
        public string richLabel;

        // public string GetRichLabel()
        // {
        //     return "<color=red>RichLabel</color>";
        // }

        [Serializable]
        private struct MyStruct
        {
            [Scene]
            public int Int;
            [AboveButton("", "above")]
            [SortingLayer]
            public string String;
            // sadly this wont work with label attribute because of the propertyField problem
            [MinMaxSlider(0f, 10f, 0.1f)] public Vector2 V2;
            public float Float;
        }

        [SerializeField]
        [RichLabel("<color=green><label /></color>")]
        // [RichLabel(null)]
        private MyStruct _myStructBadView;

        // this is a workaround
        [SerializeField]
        [AboveRichLabel("<color=green><label /></color>")]
        [RichLabel(null)]
        private MyStruct _myStructWorkAround;

        [SerializeField]
        private MyStruct _pure;
    }
}
