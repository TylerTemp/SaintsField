#if (WWISE_2030_OR_LATER || WWISE_2029_OR_LATER || WWISE_2028_OR_LATER || WWISE_2027_OR_LATER || WWISE_2026_OR_LATER || WWISE_2025_OR_LATER || WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE) && !SAINTSFIELD_WWISE_DISABLE
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Wwise.GetWwiseDrawer
{
    public partial class GetWwiseAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public bool IsReadable;
            public string Error = "";
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI =
            new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property, int index, FieldInfo info)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{index}";
            if (InfoCacheIMGUI.ContainsKey(key))
            {
                return InfoCacheIMGUI[key];
            }

            (bool isReadable, string error, SerializedProperty _) =
                GetWwiseObjectReferenceProperty(property, info);
            InfoIMGUI infoCache = new InfoIMGUI
            {
                IsReadable = isReadable,
                Error = error,
            };
            InfoCacheIMGUI[key] = infoCache;
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return infoCache;
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            InfoIMGUI infoCache = EnsureKey(property, index, info);
            return !infoCache.IsReadable || infoCache.Error != ""
                ? 0
                : base.GetPostFieldWidth(position, property, label, allAttributes, saintsAttribute, index, info,
                    parent);
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            InfoIMGUI infoCache = EnsureKey(property, index, info);
            return infoCache.IsReadable && infoCache.Error == "" &&
                   base.DrawPostFieldImGui(position, fullRect, property, label, saintsAttribute, index,
                       allAttributes, info, parent);
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            InfoIMGUI infoCache = EnsureKey(property, index, info);
            return infoCache.IsReadable &&
                   (infoCache.Error != "" ||
                    base.WillDrawBelow(property, allAttributes, saintsAttribute, index, info, parent));
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            InfoIMGUI infoCache = EnsureKey(property, index, info);
            if (!infoCache.IsReadable)
            {
                return 0;
            }

            return infoCache.Error == ""
                ? base.GetBelowExtraHeight(property, label, width, allAttributes, saintsAttribute, index, info,
                    parent)
                : ImGuiHelpBox.GetHeight(infoCache.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI infoCache = EnsureKey(property, index, info);
            if (!infoCache.IsReadable)
            {
                return position;
            }

            return infoCache.Error == ""
                ? base.DrawBelow(position, property, label, saintsAttribute, index, allAttributes, info, parent)
                : ImGuiHelpBox.Draw(position, infoCache.Error, MessageType.Error);
        }
    }
}

#endif
