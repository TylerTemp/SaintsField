using TMPro;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.UIEvents
{
    public class UIDropdown : SaintsMonoBehaviour
    {
        [OnDropdownChanged]  // bind a void
        public void DropdownChangedVoid()
        {
            Debug.Log("DropdownChanged Void");
        }

        [OnDropdownChanged]  // bind to receive the value
        public void DropdownChangedInt(int index)
        {
            Debug.Log($"DropdownChanged {index}");
        }

        [OnDropdownChanged(value: "My Custom String")]  // bind with a static value callback
        public void OtherMethodStr(string str)
        {
            Debug.Log($"OtherMethod {str}");
        }

        [GetInSiblings] public TMP_Dropdown dropdown;

        public TMP_Dropdown GetMyDropdown() => dropdown;

        [OnDropdownChanged(nameof(GetMyDropdown))] // taget is a callback (support field/property/function)
        public void BindFromOtherTarget(int dropdownIndex)
        {
            Debug.Log($"BindFromOtherTarget {dropdownIndex}");
        }
    }
}
