using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public partial class TreeDropdownAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return infoCache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            // Debug.Log($"Position {position.width}x{position.height}({position})");

            InfoIMGUI cachedInfo = EnsureKey(property);
            AdvancedDropdownMetaInfo metaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfo(property, (PathedDropdownAttribute)saintsAttribute, info, parent, true);
            cachedInfo.Error = metaInfo.Error;

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            string display = AdvancedDropdownAttributeDrawer.GetMetaStackDisplay(metaInfo);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                if (metaInfo.DropdownListValue == null)
                {
                    return;
                }

                SaintsTreeDropdownIMGUI dropdown = new SaintsTreeDropdownIMGUI(
                    metaInfo,
                    fieldRect.width,
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curItem);
                        Util.SignPropertyValue(property, info, parent, curItem);
                        property.serializedObject.ApplyModifiedProperties();
                        TriggerChangedIMGUI(property, curItem);
                        return null;
                    });

                PopupWindow.Show(fieldRect, dropdown);
            }

            // EditorGUI.DrawRect(fieldRect, Color.red);
            // var payloads = RichTextDrawer.ParseRichXmlWithProvider("<color=GoldenRod>Hi<icon=lightMeter/redLight/>", this).ToArray();
            // _richTextDrawer.DrawChunks(fieldRect, payloads);

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            // Debug.Log($"display={display}/{drawRect.width}");  // output: display=<color=GoldenRod>100%<icon=lightMeter/redLight/>
            IEnumerable<RichTextDrawer.RichTextChunk> payloads = RichTextDrawer.ParseRichXmlWithProvider(display, this);
            // foreach (RichTextDrawer.RichTextChunk richTextChunk in payloads)
            // {
            //     Debug.Log(richTextChunk);
            // }
            // EditorGUI.DrawRect(drawRect, Color.red);
            _richTextDrawer.DrawChunks(drawRect, payloads);
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureKey(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            return cachedInfo.Error == "" ? 0 : ImGuiHelpBox.GetHeight(cachedInfo.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            return cachedInfo.Error == "" ? position : ImGuiHelpBox.Draw(position, cachedInfo.Error, MessageType.Error);
        }
    }
}
