using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.MaxValueDrawer
{
    public partial class MaxValueAttributeDrawer
    {

        private class InfoImGui
        {
            public string Error = "";
            public bool CheckNow;
        }

        private static readonly Dictionary<string, InfoImGui> InfoImGuiCache = new Dictionary<string, InfoImGui>();

        private static InfoImGui EnsureKey(SerializedProperty property, int propertyIndex)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{propertyIndex}";
            if (InfoImGuiCache.TryGetValue(key, out InfoImGui infoImGui))
            {
                return infoImGui;
            }

            infoImGui = new InfoImGui
            {
                CheckNow = true,
                // CheckerResults = result,
                Error = "",
            };
            InfoImGuiCache[key] = infoImGui;

            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                InfoImGuiCache.Remove(key);
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(NotifyChange);
            });

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(NotifyChange);

            return infoImGui;

            void NotifyChange()
            {
                infoImGui.CheckNow = true;
            }
        }

        // private string _error = "";

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            MaxValueAttribute maxValueAttribute = (MaxValueAttribute)saintsAttribute;
            InfoImGui cachedInfo = EnsureKey(property, index);
            if (!cachedInfo.CheckNow)
            {
                return true;
            }
            cachedInfo.CheckNow = false;

            (IReadOnlyList<string> errors, IReadOnlyList<(string message, Action fix)> checkerResults) =
                CheckPropertyValue(property, maxValueAttribute, newValue => TriggerChangedIMGUI(property, newValue), info, parent);
            cachedInfo.Error = string.Join("\n", errors);
            foreach ((string _, Action fix)  in checkerResults)
            {
                fix.Invoke();
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureKey(property, index).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, index).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = EnsureKey(property, index).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
