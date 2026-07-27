using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.DropdownDrawer
{
    public partial class DropdownAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public readonly IMGUIUtils.IMGUITicker Ticker = new IMGUIUtils.IMGUITicker();
            public AdvancedDropdownMetaInfo MetaInfo;
            public bool Clicked;
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
            int index,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            // Debug.Log($"Position {position.width}x{position.height}({position})");

            InfoIMGUI cachedInfo = EnsureKey(property);
            // cachedInfo.Ticker ??= new IMGUIUtils.IMGUITicker();
            cachedInfo.Ticker.Tick();

            if (cachedInfo.Ticker.TickWaiterResult.Exception != null)
            {
                cachedInfo.Error = cachedInfo.Ticker.TickWaiterResult.Exception?.InnerException?.Message
                                   ?? cachedInfo.Ticker.TickWaiterResult.Exception?.Message
                                   ?? "";
                cachedInfo.Clicked = false;
            }

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            Rect labelRect = new Rect(position)
            {
                width = position.width - fieldRect.width,
            };
            DrawOverrideRichText(labelRect, label, overrideRichTextChunks);

            AdvancedDropdownMetaInfo metaInfo = cachedInfo.MetaInfo;
            bool hasMetaInfo = metaInfo.SelectStacks != null;

            GUI.SetNextControlName(FieldControlName);
            string display = hasMetaInfo ? AdvancedDropdownAttributeDrawer.GetMetaStackDisplay(metaInfo) : "";
            if (cachedInfo.Ticker.DropdownButton(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                cachedInfo.Error = "";
                cachedInfo.Clicked = true;
                cachedInfo.Ticker.ResetResolved();
            }

            bool isRunning = cachedInfo.Ticker.IsRunning();
            if (!cachedInfo.Ticker.Resolved && !isRunning)
            {
                AdvancedDropdownAttributeDrawer.GetMetaInfoAsync(cachedInfo.Ticker,
                    metaInfoRefreshed =>
                    {
                        cachedInfo.MetaInfo = metaInfoRefreshed;
                        cachedInfo.Error = metaInfoRefreshed.Error;
                        if (metaInfoRefreshed.Error != "")
                        {
                            cachedInfo.Clicked = false;
                            return;
                        }

                        if (cachedInfo.Clicked)
                        {
                            cachedInfo.Clicked = false;
                            ShowDropdown(metaInfoRefreshed);
                        }
                    },
                    property,
                    (PathedDropdownAttribute)saintsAttribute,
                    info,
                    parent,
                    true);
            }
            isRunning = cachedInfo.Ticker.IsRunning();

            // EditorGUI.DrawRect(fieldRect, Color.red);
            // var payloads = RichTextDrawer.ParseRichXmlWithProvider("<color=GoldenRod>Hi<icon=lightMeter/redLight/>", this).ToArray();
            // _richTextDrawer.DrawChunks(fieldRect, payloads);

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + (isRunning ? 22f : 6f),
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
            return;

            void ShowDropdown(AdvancedDropdownMetaInfo dropdownMetaInfo)
            {
                if (dropdownMetaInfo.DropdownListValue == null)
                {
                    cachedInfo.Clicked = false;
                    return;
                }

                SaintsTreeDropdownIMGUI dropdown = new SaintsTreeDropdownIMGUI(
                    dropdownMetaInfo,
                    fieldRect.width,
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curItem);
                        Util.SignPropertyValue(property, info, parent, curItem);
                        property.serializedObject.ApplyModifiedProperties();
                        TriggerChangedIMGUI(property, curItem);
                        cachedInfo.MetaInfo = default;
                        cachedInfo.Ticker.ResetResolved();
                        return null;
                    });

                cachedInfo.Clicked = false;
                PopupWindow.Show(fieldRect, dropdown);
            }
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
