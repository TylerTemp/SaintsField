using System;
using SaintsField.DropdownBase;
using UnityEngine;

namespace SaintsField.Samples
{
    public class DropdownExample : MonoBehaviour
    {
        // [Dropdown(nameof(GetDropdownItems))]
        // public float _float;
        // public GameObject _go1;
        // public GameObject _go2;
        // [Dropdown(nameof(GetDropdownRefs))] public GameObject _refs;
        //
        // private DropdownList<float> GetDropdownItems()
        // {
        //     return new DropdownList<float>
        //     {
        //         { "1", 1.0f },
        //         { "2", 2.0f },
        //         { "3/1", 3.1f },
        //         { "3/2", 3.2f },
        //     };
        // }
        //
        // private DropdownList<GameObject> GetDropdownRefs => new DropdownList<GameObject>
        // {
        //     {_go1.name, _go1},
        //     {_go2.name, _go2},
        //     {"NULL", null},
        // };

        [Dropdown(nameof(GetAdvancedDropdownItems))]
        public Color color;

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
    }
}