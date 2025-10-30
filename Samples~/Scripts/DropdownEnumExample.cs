using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class DropdownEnumExample : MonoBehaviour
    {
        [Serializable, Flags]
        public enum MyEnum
        {
            [FieldLabelText("1")]
            First,
            [FieldLabelText("2")]
            Second,
            [FieldLabelText("3")]
            Third,
            [FieldLabelText("4/0")]
            ForthZero,
            [FieldLabelText("4/1")]
            ForthOne,
        }

        [Dropdown] public MyEnum myEnumDropdown;
        [Dropdown(EUnique.Disable)] public MyEnum[] myEnumDropdownDisable;
        [Dropdown(EUnique.Remove)] public MyEnum[] myEnumDropdownRemove;
        [AdvancedDropdown] public MyEnum myEnumAdvancedDropdown;
        [AdvancedDropdown(EUnique.Disable)] public MyEnum[] myEnumAdvancedDropdownDisable;
        [AdvancedDropdown(EUnique.Remove)] public MyEnum[] myEnumAdvancedDropdownRemove;
    }
}
