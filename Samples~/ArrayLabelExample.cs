using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class ArrayLabelExample : MonoBehaviour
    {
        // this wont work
        [RichLabel("HI"), InfoBox("this actually wont work", EMessageType.Warning)] public int[] _ints;

        [Serializable]
        public struct MyStruct
        {
            // this works
            [RichLabel("HI"), MinMaxSlider(0f, 1f)] public Vector2 minMax;
            public float normalFloat;
        }

        public MyStruct[] myStructs;
    }
}
