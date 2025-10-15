using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class DropdownEnumExample : MonoBehaviour
    {
        [Serializable, Flags]
        public enum MyEnum
        {
            [FieldRichLabel("1")]
            First,
            [FieldRichLabel("2")]
            Second,
            [FieldRichLabel("3")]
            Third,
            [FieldRichLabel("4/0")]
            ForthZero,
            [FieldRichLabel("4/1")]
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
