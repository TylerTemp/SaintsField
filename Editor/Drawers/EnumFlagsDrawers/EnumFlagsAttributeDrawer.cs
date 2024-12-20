#if UNITY_2021_3_OR_NEWER
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public partial class EnumFlagsAttributeDrawer: SaintsPropertyDrawer
    {
        private Texture2D _checkboxCheckedTexture2D;
        private Texture2D _checkboxEmptyTexture2D;
        private Texture2D _checkboxIndeterminateTexture2D;

        private static int ToggleBit(int curValue, int bitValue)
        {
            if (EnumFlagsUtil.isOn(curValue, bitValue))
            {
                int fullBits = curValue | bitValue;
                return fullBits ^ bitValue;
            }

            // int bothOnBits = curValue & bitValue;
            // Debug.Log($"curValue={curValue}, bitValue={bitValue}, bothOnBits={bothOnBits}");
            // return bothOnBits ^ curValue;
            return curValue | bitValue;
        }

    }
}
