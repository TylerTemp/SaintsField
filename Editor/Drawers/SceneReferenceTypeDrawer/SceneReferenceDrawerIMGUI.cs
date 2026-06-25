#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SceneReferenceTypeDrawer
{
    public partial class SceneReferenceDrawer
    {
        private sealed class SceneReferenceStatusIMGUI
        {
            public string ContextError = "";
            public string HelpText = "";
            public SceneReferenceHelpAction HelpAction;
            public string ActionPath = "";
            public string ActionGuid = "";
            public SceneReferenceContext Context;
            public SceneAsset CurrentSceneAsset;
            public AdvancedDropdownMetaInfo MetaInfo;
            public bool Changed;
            public SceneReference ChangedValue;
            public bool HasTransientState;
            public string TransientPropertyGuid = "";
        }

        private const float FieldSpacing = 2f;
        private const float HelpButtonWidth = 72f;

        private static readonly Dictionary<string, SceneReferenceStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, SceneReferenceStatusIMGUI>();

        private static GUIStyle _imageButtonStyle;
        private static Texture2D _dropdownIcon;

        protected override bool UseCreateFieldIMGUI => true;

        private static GUIStyle ImageButtonStyle => _imageButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(2, 2, 2, 2),
            imagePosition = ImagePosition.ImageOnly,
            alignment = TextAnchor.MiddleCenter,
        };

        private static Texture2D DropdownIcon =>
            _dropdownIcon ??= Util.LoadResource<Texture2D>("classic-dropdown-gray.png");

        private static SceneReferenceStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out SceneReferenceStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new SceneReferenceStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            SceneReferenceStatusIMGUI cache = RefreshCache(property, false);
            return cache.ContextError == ""
                ? EditorGUIUtility.singleLineHeight
                : ImGuiHelpBox.GetHeight(cache.ContextError, width, MessageType.Error);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            SceneReferenceStatusIMGUI cache = EnsureKey(property);
            if (cache.Changed)
            {
                cache.Changed = false;
                TriggerChangedIMGUI(property, cache.ChangedValue);
            }

            RefreshCache(property, true);
            if (cache.ContextError != "")
            {
                ImGuiHelpBox.Draw(position, cache.ContextError, MessageType.Error);
                return;
            }

            Rect contentRect = EditorGUI.PrefixLabel(position, label);
            Rect labelRect = new Rect(position)
            {
                width = position.width - contentRect.width,
            };
            DrawOverrideRichText(labelRect, label, overrideRichTextChunks);
            Rect buttonRect = new Rect(contentRect)
            {
                x = contentRect.xMax - EditorGUIUtility.singleLineHeight,
                width = EditorGUIUtility.singleLineHeight,
            };
            Rect sceneRect = new Rect(contentRect)
            {
                xMax = buttonRect.xMin - FieldSpacing,
            };

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object newObject = EditorGUI.ObjectField(sceneRect, GUIContent.none, cache.CurrentSceneAsset,
                    typeof(SceneAsset), false);
                if (changed.changed)
                {
                    OnSceneObjectChanged(cache, property, newObject as SceneAsset);
                }
            }

            GUIContent dropdownContent = DropdownIcon == null
                ? GUIContent.none
                : new GUIContent(DropdownIcon, "Select Scene");
            if (!GUI.Button(buttonRect, dropdownContent, ImageButtonStyle))
            {
                return;
            }

            PopupWindow.Show(contentRect, new SaintsTreeDropdownIMGUI(
                cache.MetaInfo,
                Mathf.Max(contentRect.width, 220f),
                320f,
                false,
                (curItem, _) =>
                {
                    SceneReferencePayload payload = (SceneReferencePayload)curItem;
                    if (payload.IsAction)
                    {
                        SceneUtils.OpenBuildSettings();
                        return null;
                    }

                    SceneReference changedValue = ApplySceneReferenceGuid(cache.Context, payload.Guid);
                    ClearTransientState(cache);
                    RefreshCache(property, true);
                    cache.Changed = true;
                    cache.ChangedValue = changedValue;
                    return null;
                }));
        }

        private void OnSceneObjectChanged(SceneReferenceStatusIMGUI cache, SerializedProperty property,
            SceneAsset sceneAsset)
        {
            if (sceneAsset == null)
            {
                RefreshCache(property, true);
                return;
            }

            string guid = GetGuidFromSceneAsset(sceneAsset);
            SceneReferenceState state = GetSceneReferenceState(guid);
            if (!state.IsValidEnabledScene)
            {
                ApplyStateToCache(cache, state);
                cache.CurrentSceneAsset = sceneAsset;
                cache.HasTransientState = true;
                cache.TransientPropertyGuid = cache.Context.GuidProp.stringValue;
                return;
            }

            SceneReference changedValue = ApplySceneReferenceGuid(cache.Context, guid);
            ClearTransientState(cache);
            RefreshCache(property, true);
            TriggerChangedIMGUI(property, changedValue);
        }

        private static SceneReferenceStatusIMGUI RefreshCache(SerializedProperty property, bool updateDropdown)
        {
            SceneReferenceStatusIMGUI cache = EnsureKey(property);
            (string contextError, SceneReferenceContext context) = GetSceneReferenceContext(property);
            cache.ContextError = contextError;
            cache.Context = context;
            if (contextError != "")
            {
                cache.HelpText = "";
                cache.HelpAction = SceneReferenceHelpAction.None;
                cache.CurrentSceneAsset = null;
                cache.MetaInfo = default;
                ClearTransientState(cache);
                return cache;
            }

            RefreshGuid(context);

            bool useTransientState = cache.HasTransientState &&
                                     cache.TransientPropertyGuid == context.GuidProp.stringValue;
            if (!useTransientState)
            {
                ClearTransientState(cache);
                ApplyStateToCache(cache, GetSceneReferenceState(context.GuidProp.stringValue));
            }

            if (updateDropdown)
            {
                cache.MetaInfo = GetMetaInfo(context.GuidProp.stringValue);
            }

            return cache;
        }

        private static void ApplyStateToCache(SceneReferenceStatusIMGUI cache, SceneReferenceState state)
        {
            cache.HelpText = state.Error;
            cache.HelpAction = state.HelpAction;
            cache.ActionPath = state.ActionPath;
            cache.ActionGuid = state.ActionGuid;
            cache.CurrentSceneAsset = state.SceneAsset;
        }

        private static void ClearTransientState(SceneReferenceStatusIMGUI cache)
        {
            cache.HasTransientState = false;
            cache.TransientPropertyGuid = "";
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(string currentGuid)
        {
            AdvancedDropdownList<SceneReferencePayload> dropdown =
                new AdvancedDropdownList<SceneReferencePayload>();

            bool selected = false;
            SceneReferencePayload selectedPayload = default;
            string display = currentGuid;
            foreach (SceneReferencePayload payload in GetSceneReferencePayloads())
            {
                dropdown.Add($"[{payload.Index}] {payload.Name}", payload);
                if (payload.Guid != currentGuid)
                {
                    continue;
                }

                selected = true;
                selectedPayload = payload;
                display = $"[{payload.Index}] {payload.Name}";
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Scenes In Build...", new SceneReferencePayload("", "", -1, true), false,
                "d_editicon.sml");
            dropdown.SelfCompact();

            return new AdvancedDropdownMetaInfo
            {
                CurDisplay = display,
                CurValues = selected ? new object[] { selectedPayload } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            SceneReferenceStatusIMGUI cache = RefreshCache(property, false);
            return cache.ContextError == "" && cache.HelpText != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            SceneReferenceStatusIMGUI cache = RefreshCache(property, false);
            if (cache.ContextError != "" || cache.HelpText == "")
            {
                return 0f;
            }

            float height = ImGuiHelpBox.GetHeight(cache.HelpText, width, MessageType.Error);
            return cache.HelpAction == SceneReferenceHelpAction.None
                ? height
                : height + FieldSpacing + EditorGUIUtility.singleLineHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            SceneReferenceStatusIMGUI cache = RefreshCache(property, false);
            if (cache.ContextError != "" || cache.HelpText == "")
            {
                return position;
            }

            Rect leftRect = ImGuiHelpBox.Draw(position, cache.HelpText, MessageType.Error);
            if (cache.HelpAction == SceneReferenceHelpAction.None)
            {
                return leftRect;
            }

            Rect buttonRect = new Rect(leftRect)
            {
                y = leftRect.y + FieldSpacing,
                x = leftRect.xMax - HelpButtonWidth,
                width = HelpButtonWidth,
                height = EditorGUIUtility.singleLineHeight,
            };

            string buttonText = cache.HelpAction == SceneReferenceHelpAction.Enable ? "Enable" : "Add";
            if (GUI.Button(buttonRect, buttonText))
            {
                ApplyHelpAction(cache, property);
            }

            return new Rect(leftRect)
            {
                y = buttonRect.yMax,
                height = Mathf.Max(0f, leftRect.yMax - buttonRect.yMax),
            };
        }

        private void ApplyHelpAction(SceneReferenceStatusIMGUI cache, SerializedProperty property)
        {
            bool changedBuildSettings = cache.HelpAction == SceneReferenceHelpAction.Enable
                ? EnableScenePath(cache.ActionPath)
                : AddScenePath(cache.ActionPath);

            if (!changedBuildSettings && GetSceneReferenceState(cache.ActionGuid).HelpAction != SceneReferenceHelpAction.None)
            {
                return;
            }

            SceneReference changedValue = ApplySceneReferenceGuid(cache.Context, cache.ActionGuid);
            ClearTransientState(cache);
            RefreshCache(property, true);
            TriggerChangedIMGUI(property, changedValue);
        }
    }
}
#endif
