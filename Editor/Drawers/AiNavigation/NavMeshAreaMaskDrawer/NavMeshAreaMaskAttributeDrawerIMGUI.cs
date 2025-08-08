using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaMaskDrawer
{
    public partial class NavMeshAreaMaskAttributeDrawer
    {
        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            string[] options = AiNavigationUtils.GetNavMeshAreas().Select(each => $"{each.Mask}: {each.Name}")
                .ToArray();

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newMask = EditorGUI.MaskField(position, label, property.intValue, options);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    property.intValue = newMask;
                }
            }
        }
    }
}
