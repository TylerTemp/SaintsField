using SaintsField.DropdownBase;
using UnityEngine;

namespace SaintsField.Samples
{
    public class DropdownExample : MonoBehaviour
    {
        [Dropdown(nameof(GetDropdownItems))]
        public float _float;
        public GameObject _go1;
        public GameObject _go2;
        [Dropdown(nameof(GetDropdownRefs))] public GameObject _refs;

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

        private DropdownList<GameObject> GetDropdownRefs => new DropdownList<GameObject>
        {
            {_go1.name, _go1},
            {_go2.name, _go2},
            {"NULL", null},
        };
    }
}
