using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ButtonAddOnClickDrawer
{
    public partial class ButtonAddOnClickAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property, int index)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}[{index}]";
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return infoCache;
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => 0;

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            EnsureKey(property, index).Error = BindButtonEvent(property, saintsAttribute, info, parent);
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            EnsureKey(property, index).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, index).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, EMessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = EnsureKey(property, index).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, EMessageType.Error);
        }
    }
}
