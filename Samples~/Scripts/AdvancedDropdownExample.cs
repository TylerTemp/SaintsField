using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AdvancedDropdownExample: MonoBehaviour
    {
        public int place1;
        public int place2;
        public int place3;
        public int place4;

        [Serializable]
        public struct MyStruct
        {
            [PostFieldButton(nameof(ShowNewValue), "Click")]
            [OnValueChanged(nameof(ShowNewValue))]
            [AboveRichLabel(nameof(selectIt), true)]
            [RichLabel("<icon=star.png /><label />")]
            [AdvancedDropdown(nameof(AdvDropdown))] public int selectIt;

            public AdvancedDropdownList<int> AdvDropdown()
            {
                return new AdvancedDropdownList<int>("Days")
                {
                    // a grouped value
                    new AdvancedDropdownList<int>("First Half")
                    {
                        // with icon
                        new AdvancedDropdownList<int>("Monday", 1, icon: "eye.png"),
                        // no icon
                        new AdvancedDropdownList<int>("Tuesday", 2),
                    },
                    new AdvancedDropdownList<int>("Second Half")
                    {
                        new AdvancedDropdownList<int>("Wednesday")
                        {
                            new AdvancedDropdownList<int>("Morning", 3, icon: "eye.png"),
                            new AdvancedDropdownList<int>("Afternoon", 8),
                        },
                        new AdvancedDropdownList<int>("Thursday", 4, true, icon: "eye.png"),
                    },
                    // direct value
                    new AdvancedDropdownList<int>("Friday", 5, true),
                    AdvancedDropdownList<int>.Separator(),
                    new AdvancedDropdownList<int>("Saturday", 6, icon: "eye.png"),
                    new AdvancedDropdownList<int>("Sunday", 7, icon: "eye.png"),
                };
            }

            public void ShowNewValue()
            {
                Debug.Log($"dropIt new value: {selectIt}");
            }
        }

        public MyStruct strTyp;

        [Serializable]
        public enum ListEnum
        {
            Zero,
            One,
            Two,
            Three,
        }

        [AdvancedDropdown(nameof(ListEnumDropdown))]
        [RichLabel("$" + nameof(ListEnumLabel))]
        public ListEnum[] listEnum;

        private AdvancedDropdownList<ListEnum> ListEnumDropdown()
        {
            return new AdvancedDropdownList<ListEnum>
            {
                new AdvancedDropdownList<ListEnum>("Zero", ListEnum.Zero),
                new AdvancedDropdownList<ListEnum>("One", ListEnum.One),
                new AdvancedDropdownList<ListEnum>("Two", ListEnum.Two),
                new AdvancedDropdownList<ListEnum>("Three", ListEnum.Three),
            };
        }

        private string ListEnumLabel(ListEnum value, int index) => $"{value}/{listEnum[index]}";
    }
}
