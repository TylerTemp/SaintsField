using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfAttributeDrawer: ShowIfAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info,
            Type type, object target)
        {
            HideIfAttribute hideIfAttribute = (HideIfAttribute)saintsAttribute;
            (string error, bool shown) = base.IsShown(property, hideIfAttribute, info, type, target);
            if (error != "")
            {
                return (error, true);
            }

            Debug.Log($"HideIfAttributeDrawer: shown={shown} error={error}");

            return (error, !shown);
        }
    }
}
