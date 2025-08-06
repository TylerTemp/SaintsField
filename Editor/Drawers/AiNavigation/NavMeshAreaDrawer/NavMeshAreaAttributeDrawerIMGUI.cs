using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.AiNavigation;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaDrawer
{
    public partial class NavMeshAreaAttributeDrawer
    {
        #region IMGUI

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            AiNavigationUtils.NavMeshArea[] areas = AiNavigationUtils.GetNavMeshAreas().ToArray();
            ValueType valueType = GetValueType(property, (NavMeshAreaAttribute)saintsAttribute);

            int areaIndex = Util.ListIndexOfAction(areas, area =>
            {
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (valueType)
                {
                    case ValueType.Index:
                        return area.Value == property.intValue;
                    case ValueType.Mask:
                        return area.Mask == property.intValue;
                    case ValueType.String:
                        return area.Name == property.stringValue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null);
                }
            });

            string[] areaNames = areas
                .Select(each => FormatAreaName(each, valueType))
                .Append("")
                .Append("Open Area Settings...")
                .ToArray();

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newAreaIndex = EditorGUI.Popup(position, label.text, areaIndex, areaNames);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if (newAreaIndex < areas.Length)
                    {
                        if (valueType == ValueType.String)
                        {
                            property.stringValue = areas[newAreaIndex].Name;
                        }
                        else
                        {
                            property.intValue = valueType == ValueType.Mask
                                ? areas[newAreaIndex].Mask
                                : areas[newAreaIndex].Value;
                        }
                    }
                    else
                    {
                        NavMeshEditorHelpers.OpenAreaSettings();
                    }
                }
            }
        }

        #endregion
    }
}
