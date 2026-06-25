#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
    public partial class SaintsEventBaseDrawer
    {
        private sealed class SaintsEventStatusIMGUI
        {
            public string Error = "";
            public SaintsEventContext Context;
            public GUIContent Label = GUIContent.none;
            public ReorderableList ReorderableList;
            public UnityAction<object> ChildWatcher;
        }

        private const float FooterHeight = 20f;
        private const float FooterButtonWidth = 28f;
        private const float FooterGap = 2f;
        private static readonly Dictionary<string, SaintsEventStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, SaintsEventStatusIMGUI>();

        protected override bool UseCreateFieldIMGUI => true;

        private static SaintsEventStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out SaintsEventStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new SaintsEventStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                if (cache.ChildWatcher != null)
                {
                    RemoveChangedIMGUI(cache.ChildWatcher);
                    cache.ChildWatcher = null;
                }

                InfoCacheIMGUI.Remove(key);
            });
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            SaintsEventStatusIMGUI cache = RefreshCache(property, label, info, parent);
            if (cache.Error != "")
            {
                return ImGuiHelpBox.GetHeight(cache.Error, width, MessageType.Error);
            }

            return (cache.ReorderableList?.GetHeight() ?? EditorGUIUtility.singleLineHeight) + FooterHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            SaintsEventStatusIMGUI cache = RefreshCache(property, label, info, parent);
            if (cache.Error != "")
            {
                ImGuiHelpBox.Draw(position, cache.Error, MessageType.Error);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            try
            {
                Rect listRect = new Rect(position)
                {
                    height = Mathf.Max(0f, position.height - FooterHeight),
                };
                Rect footerRect = new Rect(position)
                {
                    y = listRect.yMax,
                    height = FooterHeight,
                };

                cache.ReorderableList?.DoList(listRect);
                DrawFooter(footerRect, cache);
            }
            catch (ObjectDisposedException)
            {
                cache.ReorderableList = null;
            }
            catch (NullReferenceException)
            {
                cache.ReorderableList = null;
            }
        }

        private SaintsEventStatusIMGUI RefreshCache(SerializedProperty property, GUIContent label, FieldInfo info,
            object parent)
        {
            SaintsEventStatusIMGUI cache = EnsureKey(property);
            (string error, SaintsEventContext context) = GetSaintsEventContext(property, label, info, parent);
            cache.Error = error;
            cache.Context = context;
            cache.Label = label == null ? GUIContent.none : new GUIContent(label);
            if (error != "")
            {
                cache.ReorderableList = null;
                return cache;
            }

            EnsureReorderableList(cache);
            EnsureChildWatcher(cache);
            return cache;
        }

        private void EnsureChildWatcher(SaintsEventStatusIMGUI cache)
        {
            if (cache.ChildWatcher != null || cache.Context?.PersistentCallsProp == null)
            {
                return;
            }

            cache.ChildWatcher = _ => ApplyAndTrigger(cache);
            WatchChangedIMGUI(cache.Context.PersistentCallsProp, cache.ChildWatcher, true);
        }

        private void EnsureReorderableList(SaintsEventStatusIMGUI cache)
        {
            SaintsEventContext context = cache.Context;
            if (context?.PersistentCallsProp == null)
            {
                return;
            }

            if (cache.ReorderableList != null)
            {
                cache.ReorderableList.footerHeight = 0f;
                return;
            }

            cache.ReorderableList = new ReorderableList(context.PersistentCallsProp.serializedObject,
                context.PersistentCallsProp, true, true, false, false)
            {
                footerHeight = 0f,
            };
            cache.ReorderableList.drawHeaderCallback += rect => DrawHeader(rect, cache);
            cache.ReorderableList.elementHeightCallback += itemIndex => GetElementHeight(cache, itemIndex);
            cache.ReorderableList.drawElementCallback += (rect, itemIndex, _, _) => DrawElement(rect, cache, itemIndex);
            cache.ReorderableList.onReorderCallbackWithDetails += (_, _, _) => ApplyAndTrigger(cache);
        }

        private static float GetElementHeight(SaintsEventStatusIMGUI cache, int itemIndex)
        {
            SerializedProperty persistentCallsProp = cache.Context.PersistentCallsProp;
            if (itemIndex < 0 || itemIndex >= persistentCallsProp.arraySize)
            {
                return 0f;
            }

            SerializedProperty itemProp = persistentCallsProp.GetArrayElementAtIndex(itemIndex);
            return EditorGUI.GetPropertyHeight(itemProp, GUIContent.none, true) +
                   EditorGUIUtility.standardVerticalSpacing;
        }

        private void DrawElement(Rect rect, SaintsEventStatusIMGUI cache, int itemIndex)
        {
            SerializedProperty persistentCallsProp = cache.Context.PersistentCallsProp;
            if (itemIndex < 0 || itemIndex >= persistentCallsProp.arraySize)
            {
                return;
            }

            SerializedProperty itemProp = persistentCallsProp.GetArrayElementAtIndex(itemIndex);
            Rect useRect = new Rect(rect)
            {
                height = Mathf.Max(0f, rect.height - EditorGUIUtility.standardVerticalSpacing),
            };

            EditorGUI.PropertyField(useRect, itemProp, GUIContent.none, true);
        }

        private void DrawHeader(Rect rect, SaintsEventStatusIMGUI cache)
        {
            GUIContent label = GetHeaderLabel(cache);
            EditorGUI.LabelField(rect, label);
            DrawOverrideRichText(rect, label, GetOverrideRichTextChunks(cache.Context.RootProperty));
        }

        private GUIContent GetHeaderLabel(SaintsEventStatusIMGUI cache)
        {
            if (overrideRichTextChunks != null)
            {
                return cache.Label ?? GUIContent.none;
            }

            return new GUIContent(cache.Context.Label ?? "");
        }

        private IEnumerable<RichTextDrawer.RichTextChunk> GetOverrideRichTextChunks(SerializedProperty property)
        {
            if (overrideRichTextChunks == null)
            {
                return null;
            }

            IReadOnlyList<Type> types = GetEventParamTypes(property);
            if (types.Count == 0)
            {
                return overrideRichTextChunks;
            }

            List<RichTextDrawer.RichTextChunk> chunks = new List<RichTextDrawer.RichTextChunk>(overrideRichTextChunks)
            {
                new RichTextDrawer.RichTextChunk(content: $" ({GetEventParamTypesLabel(types)})"),
            };
            return chunks;
        }

        private static string GetEventParamTypesLabel(IReadOnlyList<Type> types)
        {
            string[] names = new string[types.Count];
            for (int index = 0; index < types.Count; index++)
            {
                names[index] = SaintsEventUtils.StringifyType(types[index]);
            }

            return string.Join(", ", names);
        }

        private void DrawFooter(Rect rect, SaintsEventStatusIMGUI cache)
        {
            GUIStyle footerStyle = "RL Footer";
            if (Event.current.type == EventType.Repaint)
            {
                footerStyle.Draw(rect, false, false, false, false);
            }

            Rect staticRect = new Rect(rect.xMax - FooterButtonWidth, rect.y + 1f, FooterButtonWidth,
                EditorGUIUtility.singleLineHeight);
            Rect instanceRect = new Rect(staticRect.x - FooterButtonWidth - FooterGap, staticRect.y, FooterButtonWidth,
                staticRect.height);
            Rect removeRect = new Rect(instanceRect.x - FooterButtonWidth - FooterGap, staticRect.y, FooterButtonWidth,
                staticRect.height);

            using (new EditorGUI.DisabledScope(cache.Context.PersistentCallsProp.arraySize == 0))
            {
                if (GUI.Button(removeRect, "-"))
                {
                    RemoveSelected(cache);
                }
            }

            if (GUI.Button(instanceRect, "+I"))
            {
                PersistentCallAdd(cache.Context.PersistentCallsProp, false);
                ApplyAndTrigger(cache);
            }

            GUIStyle staticStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = new Color(1f, 0.65f, 0f) },
            };
            if (GUI.Button(staticRect, "+S", staticStyle))
            {
                PersistentCallAdd(cache.Context.PersistentCallsProp, true);
                ApplyAndTrigger(cache);
            }
        }

        private void RemoveSelected(SaintsEventStatusIMGUI cache)
        {
            SaintsEventContext context = cache.Context;
            int deleteIndex = cache.ReorderableList.index >= 0
                ? cache.ReorderableList.index
                : context.PersistentCallsProp.arraySize - 1;
            if (deleteIndex < 0 || deleteIndex >= context.PersistentCallsProp.arraySize)
            {
                return;
            }

            context.PersistentCallsProp.DeleteArrayElementAtIndex(deleteIndex);
            context.PersistentCallsProp.serializedObject.ApplyModifiedProperties();
            cache.ReorderableList.index = context.PersistentCallsProp.arraySize == 0
                ? -1
                : Mathf.Clamp(deleteIndex, 0, context.PersistentCallsProp.arraySize - 1);
            ApplyAndTrigger(cache);
        }

        private void ApplyAndTrigger(SaintsEventStatusIMGUI cache)
        {
            SaintsEventContext context = cache.Context;
            context.RootProperty.serializedObject.ApplyModifiedProperties();
            (string error, int _, object value) = Util.GetValue(context.RootProperty, context.Info, context.Parent);
            if (error == "")
            {
                TriggerChangedIMGUI(context.RootProperty, value);
            }
        }
    }
}
#endif
