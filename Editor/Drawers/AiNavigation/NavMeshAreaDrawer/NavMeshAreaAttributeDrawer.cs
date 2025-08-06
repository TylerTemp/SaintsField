using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.AiNavigation;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaDrawer
{


#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(NavMeshAreaAttribute), true)]
    public partial class NavMeshAreaAttributeDrawer: SaintsPropertyDrawer
    {
        private enum ValueType
        {
            Mask,
            Index,
            String,
        }

        private static ValueType GetValueType(SerializedProperty property, NavMeshAreaAttribute navMeshAreaAttribute)
        {
            if(property.propertyType == SerializedPropertyType.Integer)
            {
                return navMeshAreaAttribute.IsMask
                    ? ValueType.Mask
                    : ValueType.Index;
            }
            return ValueType.String;
        }

        private static string FormatAreaName(AiNavigationUtils.NavMeshArea area, ValueType valueType) =>
            $"{(valueType == ValueType.Mask ? area.Mask : area.Value)}: {area.Name}";

    }
}
