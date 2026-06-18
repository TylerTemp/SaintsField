using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ReferencePicker;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
    public partial class SaintsInterfaceDrawer
    {
        private sealed class InterfaceStatusIMGUI
        {
            public string Error = "";
            public InterfaceFieldInfo FieldInfo;
            public string ReferenceLabel = "";
            public AdvancedDropdownMetaInfo ReferenceMetaInfo;
            public readonly SaintsRowAttributeDrawer.ManagedReferenceBodyInfo BodyInfo =
                new SaintsRowAttributeDrawer.ManagedReferenceBodyInfo();
            public bool ExpandStateInitialized;
            public bool PendingManagedReferenceChanged;
            public UnityAction<object> ManagedReferenceWatcher;
        }

        private const float ModeButtonWidth = 20f;
        private const float PickerButtonWidth = 18f;
        private const float FoldoutWidth = 12f;
        private const float FieldSpacing = 2f;

        private static readonly Dictionary<string, InterfaceStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, InterfaceStatusIMGUI>();

        private static GUIStyle _modeButtonStyle;
        private static GUIStyle _imageButtonStyle;
        private static Texture2D _pickerIcon;

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        protected override bool UseCreateFieldIMGUI => true;

        private static GUIStyle ModeButtonStyle => _modeButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
        {
            richText = true,
            padding = new RectOffset(0, 0, 0, 0),
        };

        private static Texture2D PickerIcon =>
            _pickerIcon ??= EditorGUIUtility.IconContent("d_pick_uielements").image as Texture2D;

        private static GUIStyle ImageButtonStyle => _imageButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(2, 2, 2, 2),
            imagePosition = ImagePosition.ImageOnly,
            alignment = TextAnchor.MiddleCenter,
        };

        private static void ClearCacheInfo(InterfaceStatusIMGUI cache)
        {
            cache.Error = "";
            cache.FieldInfo = default;
            cache.ReferenceLabel = "";
            cache.ReferenceMetaInfo = default;
            cache.ExpandStateInitialized = false;
            cache.PendingManagedReferenceChanged = false;
            SaintsRowAttributeDrawer.ClearManagedReferenceBody(cache.BodyInfo);
        }

        private static InterfaceStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InterfaceStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InterfaceStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                if (cache.ManagedReferenceWatcher != null)
                {
                    RemoveChangedIMGUI(cache.ManagedReferenceWatcher);
                    cache.ManagedReferenceWatcher = null;
                }

                ClearCacheInfo(cache);
                InfoCacheIMGUI.Remove(key);
            });
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            InterfaceStatusIMGUI cache = EnsureKey(property);
            try
            {
                RefreshCache(cache, property, info, parent);
            }
            catch (InvalidOperationException e)
            {
                cache.Error = e.Message;
            }

            if (cache.Error != "")
            {
                return SingleLineHeight;
            }

            if (!cache.FieldInfo.IsVRefProp.boolValue || !cache.FieldInfo.VRefProp.isExpanded)
            {
                return SingleLineHeight;
            }

            return SingleLineHeight +
                   SaintsRowAttributeDrawer.GetManagedReferenceBodyHeight(cache.BodyInfo, width - IndentWidth);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, int index,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            InterfaceStatusIMGUI cache = EnsureKey(property);
            try
            {
                RefreshCache(cache, property, info, parent);
            }
            catch (InvalidOperationException e)
            {
                cache.Error = e.Message;
            }

            if (cache.Error != "")
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                return;
            }

            Rect lineRect = new Rect(position)
            {
                height = SingleLineHeight,
            };
            Rect contentRect = EditorGUI.PrefixLabel(lineRect, label);
            Rect modeRect = new Rect(contentRect)
            {
                width = ModeButtonWidth,
            };

            if (GUI.Button(modeRect, GetModeContent(cache.FieldInfo.IsVRefProp.boolValue), ModeButtonStyle))
            {
                bool nextIsVRef = !cache.FieldInfo.IsVRefProp.boolValue;
                bool changed = cache.FieldInfo.IsVRefProp.boolValue != nextIsVRef;
                cache.FieldInfo.IsVRefProp.boolValue = nextIsVRef;
                changed |= SyncInterfaceModeSideEffectsWithoutApply(cache.FieldInfo.ValueProp,
                    cache.FieldInfo.VRefProp, nextIsVRef);
                if (changed)
                {
                    cache.FieldInfo.IsVRefProp.serializedObject.ApplyModifiedProperties();
                    RefreshCache(cache, property, info, parent);
                    TriggerChangedIMGUI(property, GetCurrentValue(cache));
                }
            }

            Rect activeRect = new Rect(contentRect)
            {
                xMin = modeRect.xMax + FieldSpacing,
            };
            if (cache.FieldInfo.IsVRefProp.boolValue)
            {
                DrawReferenceField(position, activeRect, property, info, parent, cache);
            }
            else
            {
                DrawObjectField(activeRect, property, info, parent, cache);
            }
        }

        private void RefreshCache(InterfaceStatusIMGUI cache, SerializedProperty property, FieldInfo info,
            object parent)
        {
            cache.FieldInfo = GetInterfaceFieldInfo(property, info);
            cache.Error = cache.FieldInfo.Error;
            if (cache.Error != "")
            {
                SaintsRowAttributeDrawer.ClearManagedReferenceBody(cache.BodyInfo);
                return;
            }

            EnsureManagedReferenceWatcher(cache);
            EnsureExpandState(cache, info);

            cache.ReferenceLabel = Util.GetReferencePropertyLabel(cache.FieldInfo.VRefProp);
            cache.ReferenceMetaInfo = GetReferenceMetaInfo(cache.FieldInfo.VRefProp,
                GetTypesImplementingInterface(cache.FieldInfo.InterfaceType));

            SaintsRowAttributeDrawer.SyncManagedReferenceBody(cache.BodyInfo, cache.FieldInfo.VRefProp, info, parent,
                cache.FieldInfo.VRefProp.managedReferenceValue, this);
            cache.Error = "";
        }

        private void EnsureManagedReferenceWatcher(InterfaceStatusIMGUI cache)
        {
            if (cache.ManagedReferenceWatcher != null || cache.FieldInfo.VRefProp == null)
            {
                return;
            }

            cache.ManagedReferenceWatcher = _ => cache.PendingManagedReferenceChanged = true;
            WatchChangedIMGUI(cache.FieldInfo.VRefProp, cache.ManagedReferenceWatcher, true);
        }

        private static void EnsureExpandState(InterfaceStatusIMGUI cache, MemberInfo info)
        {
            if (cache.ExpandStateInitialized || cache.FieldInfo.VRefProp == null)
            {
                return;
            }

            cache.FieldInfo.VRefProp.isExpanded =
                ShouldReferenceStartExpanded(ReflectCache.GetCustomAttributes<Attribute>(info),
                    cache.FieldInfo.VRefProp);
            cache.ExpandStateInitialized = true;
        }

        private static GUIContent GetModeContent(bool isVRef) => new GUIContent(
            isVRef ? "<color=#ffa500>R</color>" : "<color=#00ffff>I</color>",
            isVRef ? "Serializable Reference" : "Unity Instance");

        private static object GetCurrentValue(InterfaceStatusIMGUI cache) => cache.FieldInfo.IsVRefProp.boolValue
            ? cache.FieldInfo.VRefProp.managedReferenceValue
            : cache.FieldInfo.ValueProp.objectReferenceValue;

        private void DrawObjectField(Rect position, SerializedProperty property, FieldInfo info, object parent,
            InterfaceStatusIMGUI cache)
        {
            Rect pickerRect = new Rect(position)
            {
                x = position.xMax - PickerButtonWidth,
                width = PickerButtonWidth,
            };
            Rect fieldRect = new Rect(position)
            {
                xMax = pickerRect.xMin,
            };

            Object oldValue = cache.FieldInfo.ValueProp.objectReferenceValue;
            EditorGUI.BeginChangeCheck();
            Object newValue = EditorGUI.ObjectField(fieldRect, GUIContent.none, oldValue, cache.FieldInfo.ValueType,
                true);
            if (EditorGUI.EndChangeCheck())
            {
                if (!TryGetMatchedInterfaceValue(newValue, cache.FieldInfo.ValueType, cache.FieldInfo.InterfaceType,
                        out Object matchedValue))
                {
                    matchedValue = oldValue;
                }

                if (cache.FieldInfo.ValueProp.objectReferenceValue != matchedValue)
                {
                    cache.FieldInfo.ValueProp.objectReferenceValue = matchedValue;
                    cache.FieldInfo.ValueProp.serializedObject.ApplyModifiedProperties();
                    RefreshCache(cache, property, info, parent);
                    TriggerChangedIMGUI(property, matchedValue);
                }
            }

            GUIContent pickerContent = PickerIcon == null
                ? GUIContent.none
                : new GUIContent(PickerIcon, "Select");
            if (!GUI.Button(pickerRect, pickerContent, ImageButtonStyle))
            {
                return;
            }

            FieldInterfaceSelectWindow.Open(cache.FieldInfo.ValueProp.objectReferenceValue, cache.FieldInfo.ValueType,
                cache.FieldInfo.InterfaceType, fieldResult =>
                {
                    if (cache.FieldInfo.ValueProp.objectReferenceValue == fieldResult)
                    {
                        return;
                    }

                    cache.FieldInfo.ValueProp.objectReferenceValue = fieldResult;
                    cache.FieldInfo.ValueProp.serializedObject.ApplyModifiedProperties();
                    RefreshCache(cache, property, info, parent);
                    TriggerChangedIMGUI(property, fieldResult);
                });
        }

        private void DrawReferenceField(Rect position, Rect lineContentRect, SerializedProperty property, FieldInfo info,
            object parent, InterfaceStatusIMGUI cache)
        {
            Rect foldoutRect = new Rect(lineContentRect)
            {
                width = FoldoutWidth,
            };
            bool expanded = GUI.Toggle(foldoutRect, cache.FieldInfo.VRefProp.isExpanded, GUIContent.none,
                EditorStyles.foldout);
            if (expanded != cache.FieldInfo.VRefProp.isExpanded)
            {
                cache.FieldInfo.VRefProp.isExpanded = expanded;
            }

            Rect popupRect = new Rect(lineContentRect)
            {
                xMin = foldoutRect.xMax + FieldSpacing,
            };

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(popupRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(popupRect, new SaintsTreeDropdownIMGUI(
                    cache.ReferenceMetaInfo,
                    Mathf.Max(popupRect.width, 220f),
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        object currentValue = cache.FieldInfo.VRefProp.managedReferenceValue;
                        object instance = curItem == null
                            ? null
                            : ReferencePickerAttributeDrawer.CopyObj(currentValue,
                                Activator.CreateInstance((Type)curItem));

                        cache.FieldInfo.VRefProp.managedReferenceValue = instance;
                        cache.FieldInfo.VRefProp.serializedObject.ApplyModifiedProperties();
                        RefreshCache(cache, property, info, parent);
                        TriggerChangedIMGUI(property, instance);
                        return null;
                    }));
            }

            Rect drawRect = new Rect(popupRect)
            {
                xMin = popupRect.xMin + 6f,
                xMax = popupRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect,
                RichTextDrawer.ParseRichXmlWithProvider(cache.ReferenceLabel,
                    new RichTextDrawer.EmptyRichTextTagProvider()));

            if (!cache.FieldInfo.VRefProp.isExpanded)
            {
                cache.PendingManagedReferenceChanged = false;
                return;
            }

            Rect childRect = new Rect(position)
            {
                x = position.x + IndentWidth,
                y = position.y + SingleLineHeight,
                width = position.width - IndentWidth,
                height = position.height - SingleLineHeight,
            };
            SaintsRowAttributeDrawer.DrawManagedReferenceBody(childRect, cache.BodyInfo);

            if (!cache.PendingManagedReferenceChanged)
            {
                return;
            }

            cache.PendingManagedReferenceChanged = false;
            TriggerChangedIMGUI(property, cache.FieldInfo.VRefProp.managedReferenceValue);
        }

        private static AdvancedDropdownMetaInfo GetReferenceMetaInfo(SerializedProperty property,
            IReadOnlyList<Type> implementingTypes)
        {
            Dropdown<Type> dropdown = new Dropdown<Type>
            {
                { "[Null]", null },
            };
            Dictionary<string, List<Type>> namespaceToTypes = new Dictionary<string, List<Type>>();
            foreach (Type type in implementingTypes)
            {
                string typeNamespace = type.Namespace ?? "";
                if (!namespaceToTypes.TryGetValue(typeNamespace, out List<Type> groupedTypes))
                {
                    namespaceToTypes[typeNamespace] = groupedTypes = new List<Type>();
                }

                groupedTypes.Add(type);
            }

            foreach (string typeNamespace in namespaceToTypes.Keys.OrderBy(each => each))
            {
                Dropdown<Type> namespaceDropdown =
                    new Dropdown<Type>(typeNamespace == "" ? "[No Namespace]" : typeNamespace);
                foreach (Type type in namespaceToTypes[typeNamespace])
                {
                    namespaceDropdown.Add(type.Name, type);
                }

                dropdown.Add(namespaceDropdown);
            }

            object currentValue = property.managedReferenceValue;
            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurDisplay = currentValue == null ? "-" : currentValue.GetType().Name,
                CurValues = currentValue == null ? Array.Empty<object>() : new object[] { currentValue.GetType() },
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent) => EnsureKey(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            InterfaceStatusIMGUI cache = EnsureKey(property);
            return cache.Error == "" ? 0 : ImGuiHelpBox.GetHeight(cache.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InterfaceStatusIMGUI cache = EnsureKey(property);
            return cache.Error == "" ? position : ImGuiHelpBox.Draw(position, cache.Error, MessageType.Error);
        }
    }
}
