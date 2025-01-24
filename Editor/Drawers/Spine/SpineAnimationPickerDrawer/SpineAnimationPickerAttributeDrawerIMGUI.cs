using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine.SpineAnimationPickerDrawer
{
    public partial class SpineAnimationPickerAttributeDrawer
    {
        private class CachedInfo
        {
            public string Error = "";
        }

        private static Dictionary<string, CachedInfo> _cachedInfo = new Dictionary<string, CachedInfo>();

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if(!_cachedInfo.TryGetValue(property.propertyPath, out CachedInfo cachedInfo))
            {
                string key = SerializedUtils.GetUniqueId()
                cachedInfo = new CachedInfo();
                _cachedInfo[property.propertyPath] = cachedInfo;
            }

            return EditorGUIUtility.singleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            Draw(position, property, label, saintsAttribute, info, parent);
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return GetDisplayError(property) != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string displayError = GetDisplayError(property);
            return displayError == "" ? 0 : ImGuiHelpBox.GetHeight(displayError, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string displayError = GetDisplayError(property);
            return displayError == ""
                ? position
                : ImGuiHelpBox.Draw(position, displayError, MessageType.Error);
        }
    }
}
