using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.BuiltIn
{
    public static class DelayedAttributeDrawer
    {
        public static void ApplyDelayed<T>(TextInputBaseField<T> element, bool hasDelayed)
        {
            if (hasDelayed)
            {
                element.isDelayed = true;
            }
        }
    }
}
