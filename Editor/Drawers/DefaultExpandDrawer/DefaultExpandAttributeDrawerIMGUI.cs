using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.DefaultExpandDrawer
{
    public partial class DefaultExpandAttributeDrawer
    {
        private static readonly HashSet<string> WatchedProp = new HashSet<string>();

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return true;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            return 0f;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (WatchedProp.Contains(key))
            {
                return position;
            }

            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                WatchedProp.Remove(key);
            });
            WatchedProp.Add(key);
            property.isExpanded = true;
            return position;
        }
    }
}
