using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.PostFieldRichLabelDrawer
{
    public partial class PostFieldRichLabelAttributeDrawer
    {
        private class ImGuiCacheInfo
        {
            public readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();
            public string Error = "";
        }

        private static readonly Dictionary<string, ImGuiCacheInfo> ImGuiCache = new Dictionary<string, ImGuiCacheInfo>();

        private static ImGuiCacheInfo EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once InvertIf
            if (!ImGuiCache.TryGetValue(key, out ImGuiCacheInfo info))
            {
                ImGuiCache[key] = info = new ImGuiCacheInfo();

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    // ReSharper disable once InvertIf
                    if (ImGuiCache.TryGetValue(key, out info))
                    {
                        info.RichTextDrawer.Dispose();
                        ImGuiCache.Remove(key);
                    }
                });
            }

            return info;
        }

        // private string _error = "";
        private IReadOnlyList<RichTextDrawer.RichTextChunk> _payloads;

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            PostFieldRichLabelAttribute targetAttribute = (PostFieldRichLabelAttribute)saintsAttribute;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml,
                targetAttribute.IsCallback, info, parent);

            ImGuiCacheInfo cache = EnsureKey(property);

            cache.Error = error;

            if (error != "" || string.IsNullOrEmpty(xml))
            {
                _payloads = null;
                return 0;
            }

            _payloads = RichTextDrawer.ParseRichXml(xml, label.text, property, info, parent).ToArray();
            return cache.RichTextDrawer.GetWidth(label, position.height, _payloads) + targetAttribute.Padding;
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            ImGuiCacheInfo cache = EnsureKey(property);
            if (cache.Error != "")
            {
                return false;
            }

            if (_payloads == null || _payloads.Count == 0)
            {
                return false;
            }

            PostFieldRichLabelAttribute targetAttribute = (PostFieldRichLabelAttribute)saintsAttribute;

            Rect drawRect = new Rect(position)
            {
                x = position.x + targetAttribute.Padding,
                width = position.width - targetAttribute.Padding,
            };

            ImGuiEnsureDispose(property.serializedObject.targetObject);
            cache.RichTextDrawer.DrawChunks(drawRect, label, _payloads);

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return EnsureKey(property).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
