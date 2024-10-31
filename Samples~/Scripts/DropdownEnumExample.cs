using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class DropdownEnumExample : MonoBehaviour
    {
        [Serializable, Flags]
        public enum MyEnum
        {
            [RichLabel("1")]
            First,
            [RichLabel("2")]
            Second,
            [RichLabel("3")]
            Third,
            [RichLabel("4/0")]
            ForthZero,
            [RichLabel("4/1")]
            ForthOne,
        }

        [Dropdown] public MyEnum myEnum;
    }
}
