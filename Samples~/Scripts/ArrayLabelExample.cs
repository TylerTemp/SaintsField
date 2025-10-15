using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ArrayLabelExample : MonoBehaviour
    {
        // FIXED: list above/below incorrect height when item height is not the same

        [FieldRichLabel(nameof(IntsLabel), true)]
        [FieldAboveText(nameof(IntsLabel), true)]
        [BelowRichLabel(nameof(IntsLabel), true)]
        public int[] ints;

        private string IntsLabel(int value, int index)
        {
            if (index <= 1)
            {
                return null;
            }

            return (value < 0? "<color=red>": "") + $"<label/>[{index}]={value}";
        }

        [Serializable]
        public struct MyStruct
        {
            // this works
            [FieldRichLabel("HI"), MinMaxSlider(0f, 1f)] public Vector2 minMax;
            public float normalFloat;
        }

        public MyStruct[] myStructs;

        [Serializable]
        public struct StructNested
        {
            [FieldRichLabel(nameof(IntsLabel), true)]
            [FieldAboveText(nameof(IntsLabel), true)]
            [BelowRichLabel(nameof(IntsLabel), true)]
            public int[] ints;

            private string IntsLabel(int value, int index)
            {
                if (index <= 1)
                {
                    return null;
                }

                return (value < 0? "<color=red>": "") + $"[{index}]={value}|<label/>";
            }
        }

        public StructNested[] structNested;
    }
}
