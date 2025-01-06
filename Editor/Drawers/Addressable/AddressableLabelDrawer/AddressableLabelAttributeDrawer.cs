#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
    [CustomPropertyDrawer(typeof(AddressableLabelAttribute))]
    public partial class AddressableLabelAttributeDrawer: SaintsPropertyDrawer
    {
        // private static string ErrorNoSettings => "Addressable has no settings created yet.";


#if UNITY_2021_3_OR_NEWER


#endif
    }
}
#endif
