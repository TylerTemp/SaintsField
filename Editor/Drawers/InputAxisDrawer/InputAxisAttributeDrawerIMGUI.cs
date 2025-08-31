using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.InputAxisDrawer
{
    public partial class InputAxisAttributeDrawer
    {

        private IReadOnlyList<string> _axisNames;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_axisNames == null)
            {
                _axisNames = InputAxisUtils.GetAxisNames();
            }

            int index = IndexOf(_axisNames, property.stringValue);
            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                GUIContent optionContent = new GUIContent("Open Input Manager...");
                int newIndex = EditorGUI.Popup(position, label, index, _axisNames
                    .Select(each => new GUIContent(each))
                    .Concat(new[] { GUIContent.none, optionContent })
                    .ToArray());
                // ReSharper disable once InvertIf
                if (changeCheck.changed)
                {
                    if (newIndex >= _axisNames.Count)
                    {
                        InputAxisUtils.OpenInputManager();
                        return;
                    }

                    property.stringValue = _axisNames[newIndex];
                }
            }
        }

        private static int IndexOf(IEnumerable<string> axisNames, string value)
        {
            // int index = Array.IndexOf(scenes, scene);
            // return index == -1? 0: index;
            foreach ((string axisName, int index) in axisNames.Select((axisName, index) => (axisName, index)))
            {
                if (axisName == value)
                {
                    return index;
                }
            }

            return -1;
        }

    }
}
