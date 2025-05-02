using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DisabledDrawers.ReadOnlyDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
    [CustomPropertyDrawer(typeof(DisableIfAttribute), true)]
    public partial class ReadOnlyAttributeDrawer: SaintsPropertyDrawer
    {
        // protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        // {
        //     (string error, bool disabled) = IsDisabled(property, (ReadOnlyAttribute)saintsAttribute, info, parent.GetType(), parent);
        //     _error = error;
        //     EditorGUI.BeginDisabledGroup(disabled);
        //     return -1;
        // }

        // protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        // {
        //     EditorGUI.EndDisabledGroup();
        //     return true;
        // }

    }
}
