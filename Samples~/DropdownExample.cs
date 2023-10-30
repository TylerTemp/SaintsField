using System;
using ExtInspector;
using ExtInspector.DropdownBase;
using UnityEngine;

namespace Samples
{
    public class DropdownExample : MonoBehaviour
    {
        [SerializeField, Dropdown(nameof(GetDropdownItems))] private float _float;
        [field: SerializeField, Dropdown(nameof(GetDropdownRefs))] private GameObject _refs;

        [SerializeField] private GameObject _go1;
        [SerializeField] private GameObject _go2;

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

        // [Serializable]
        // private struct ExampleStruct
        // {
        //     public string Name;
        //     public float Value;
        // }

        private DropdownList<GameObject> GetDropdownRefs => new DropdownList<GameObject>
        {
            {_go1.name, _go1},
            {_go2.name, _go2},
            {"NULL", null},
        };
    }
}
