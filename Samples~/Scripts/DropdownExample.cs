using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class DropdownExample : MonoBehaviour
    {
        [SerializeField] private string _label;

        [Dropdown(nameof(GetDropdownItems)), RichLabel(nameof(GetLabel), true)]
        public float floatV;

        private string GetLabel() => string.IsNullOrEmpty(_label) ? null : _label;

        private DropdownList<float> GetDropdownItems()
        {
            return new DropdownList<float>
            {
                { "1", 1.0f },
                { "2", 2.0f },
                { "3/1", 3.1f },
                { "3/2", 3.2f },
            };
        }

        public GameObject go1;
        public GameObject go2;
        [Dropdown(nameof(GetDropdownRefs))][RichLabel("<icon=star.png /><label />")]
        public GameObject refs;

        private DropdownList<GameObject> GetDropdownRefs => new DropdownList<GameObject>
        {
            // ReSharper disable once Unity.NoNullCoalescing
            {go1.name, go1 ?? null},
            // ReSharper disable once Unity.NoNullCoalescing
            {go2.name, go2 ?? null},
            {"NULL", null},
        };

        [Dropdown(nameof(GetAdvancedDropdownItems))]
        public Color color;
        [Dropdown(nameof(GetAdvancedDropdownItems), false)]
        public Color colorNoSub;

        private DropdownList<Color> GetAdvancedDropdownItems()
        {
            return new DropdownList<Color>
            {
                { "Black", Color.black },
                { "White", Color.white },
                DropdownList<Color>.Separator(),
                { "Basic/Red", Color.red, true },
                { "Basic/Green", Color.green },
                { "Basic/Blue", Color.blue },
                DropdownList<Color>.Separator("Basic/"),
                { "Basic/Magenta", Color.magenta },
                { "Basic/Cyan", Color.cyan },
            };
        }

        [Dropdown(nameof(GetUniqItems))] public string uniq1;
        [Dropdown(nameof(GetUniqItems))] public string uniq2;
        [Dropdown(nameof(GetUniqItems))] public string uniq3;

        private DropdownList<string> GetUniqItems() => new DropdownList<string>
        {
            { "One", "1", Array.IndexOf(new[]{uniq1, uniq2, uniq3}, "1") != -1 },
            { "Two", "2" , Array.IndexOf(new[]{uniq1, uniq2, uniq3}, "2") != -1 },
            { "Three", "3" , Array.IndexOf(new[]{uniq1, uniq2, uniq3}, "3") != -1 },
            { "Four", "4" , Array.IndexOf(new[]{uniq1, uniq2, uniq3}, "4") != -1 },
        };

        // dropdown under stuck

        public int normal;

        // [OnValueChanged(nameof(OnChanged))]
        [Dropdown(nameof(MyStructValues))]
        // [BelowRichLabel(nameof(GetValue), true)]
        public int myInt;

        private DropdownList<int> MyStructValues => new DropdownList<int>
        {
            { "v0", 0 },
            { "v1", 1 },
            { "v2", 2 },
            { "v3", 3 },
        };

        [Serializable]
        private struct MyData
        {
            public int normal;

            // [OnValueChanged(nameof(OnChanged))]
            [Dropdown(nameof(MyStructValues))]
            [BelowRichLabel(nameof(myInt), true)]
            public int myInt;

            private DropdownList<int> MyStructValues => new DropdownList<int>
            {
                { "v0", 0 },
                { "v1", 1 },
                { "v2", 2 },
                { "v3", 3 },
            };

            // private string GetValue() => myInt;
            private string GetValue() => myInt.ToString();
            private void OnChanged() => Debug.Log(myInt.ToString());
        }

        [SerializeField] private MyData my;

        [ReadOnly]
        [Dropdown(nameof(GetDropdownItems))]
        public float floatVDisabled;

        [Serializable]
        public struct MyStruct
        {
            public GameObject go;
            public int[] someIntegers;
        }

        [Serializable]
        public struct MyStructParent
        {
            public GameObject go;
            public int[] someIntegers;
            public MyStruct myStruct;
            public MyStruct[] myStructs;
        }

        [InfoBox("This works fine, but because we use Equal to compare the struct, it will by default not equal, and does not know when it's actually the same struct")]
        [Dropdown(nameof(MyStructParentDropdown))]
        public MyStructParent nestedStruct;

        public DropdownList<MyStructParent> MyStructParentDropdown() => new DropdownList<MyStructParent>
        {
            {"value", new MyStructParent
            {
                go = gameObject,
                someIntegers = new[] {1, 2, 3},
                myStruct = new MyStruct
                {
                    go = gameObject,
                    someIntegers = new[] {4, 5, 6},
                },
                myStructs = new[]
                {
                    new MyStruct
                    {
                        go = gameObject,
                        someIntegers = new[] {7, 8, 9},
                    },
                    new MyStruct
                    {
                        go = gameObject,
                        someIntegers = new[] {10, 11, 12},
                    },
                },
            }},
            // {"value2", new MyStructParent
            // {
            //     go = gameObject,
            //     someIntegers = new[] {1, 2, 3},
            //     // myStruct = new MyStruct
            //     // {
            //     //     go = gameObject,
            //     //     someIntegers = new[] {4, 5, 6},
            //     // },
            //     myStructs = new[]
            //     {
            //         new MyStruct
            //         {
            //             go = gameObject,
            //             someIntegers = new[] {7, 8, 9},
            //         },
            //         new MyStruct
            //         {
            //             go = gameObject,
            //             someIntegers = new[] {10, 11, 12},
            //         },
            //     },
            // }},
        };

        [Serializable]
        public enum ListEnum
        {
            Zero,
            One,
            Two,
            Three,
        }


        [Dropdown(nameof(ListEnumDropdown))]
        [RichLabel("$" + nameof(ListEnumLabel))]
        public ListEnum[] listEnum;

        private DropdownList<ListEnum> ListEnumDropdown()
        {
            return new DropdownList<ListEnum>
            {
                {"Zero", ListEnum.Zero},
                {"One", ListEnum.One},
                {"Two", ListEnum.Two},
                {"Three", ListEnum.Three},
            };
        }

        private string ListEnumLabel(ListEnum value, int index) => $"{value}/{listEnum[index]}";
    }
}
