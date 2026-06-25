using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ReferencePicker;
using SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsWrapTypeDrawer
{
    public partial class BaseWrapDrawer
    {
        private sealed class FieldStatusIMGUI
        {
            public string Error = "";
            public Type WrappedType;
            public SerializedProperty ValueFieldProp;
            public SerializedProperty ValueProp;
            public SerializedProperty IsVRefProp;
            public SerializedProperty VRefProp;
            public string ReferenceLabel = "";
            public AdvancedDropdownMetaInfo ReferenceMetaInfo;
            public readonly SaintsRowAttributeDrawer.ManagedReferenceBodyInfo BodyInfo =
                new SaintsRowAttributeDrawer.ManagedReferenceBodyInfo();
            public bool ExpandStateInitialized;
            public bool PendingManagedReferenceChanged;
            public UnityAction<object> ManagedReferenceWatcher;
        }

        private const float ModeButtonWidth = 20f;
        private const float FoldoutWidth = 12f;
        private const float FieldSpacing = 2f;

        private static readonly Dictionary<string, FieldStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, FieldStatusIMGUI>();

        private static GUIStyle _modeButtonStyle;
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        protected override bool UseCreateFieldIMGUI => true;

        private static GUIStyle ModeButtonStyle => _modeButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
        {
            richText = true,
            padding = new RectOffset(0, 0, 0, 0),
        };

        private static FieldStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out FieldStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new FieldStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                if (cache.ManagedReferenceWatcher != null)
                {
                    RemoveChangedIMGUI(cache.ManagedReferenceWatcher);
                    cache.ManagedReferenceWatcher = null;
                }

                SaintsRowAttributeDrawer.ClearManagedReferenceBody(cache.BodyInfo);
                InfoCacheIMGUI.Remove(key);
            });
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            if (GetWrapMode(property) != WrapType.Field)
            {
                (SerializedProperty realProp, FieldInfo _) = GetBasicInfo(property, info);
                return realProp == null
                    ? SingleLineHeight
                    : EditorGUI.GetPropertyHeight(realProp, GUIContent.none, true);
            }

            FieldStatusIMGUI cache = EnsureKey(property);
            RefreshFieldCache(cache, property, info, parent);
            if (cache.Error != "")
            {
                return ImGuiHelpBox.GetHeight(cache.Error, width, MessageType.Error);
            }

            if (!cache.IsVRefProp.boolValue || !cache.VRefProp.isExpanded)
            {
                return SingleLineHeight;
            }

            return SingleLineHeight +
                   SaintsRowAttributeDrawer.GetManagedReferenceBodyHeight(cache.BodyInfo, width - IndentWidth);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            if (GetWrapMode(property) != WrapType.Field)
            {
                (SerializedProperty realProp, FieldInfo _) = GetBasicInfo(property, info);
                if (realProp != null)
                {
                    EditorGUI.PropertyField(position, realProp, GUIContent.none, true);
                    DrawOverrideRichText(position, label, overrideRichTextChunks);
                }
                return;
            }

            FieldStatusIMGUI cache = EnsureKey(property);
            RefreshFieldCache(cache, property, info, parent);
            if (cache.Error != "")
            {
                ImGuiHelpBox.Draw(position, cache.Error, MessageType.Error);
                return;
            }

            bool canNotAssignUnityObject =
                cache.WrappedType.IsClass && !typeof(Object).IsAssignableFrom(cache.WrappedType)
                || cache.WrappedType.IsValueType;
            bool canOnlyAssignUnityObject =
                cache.WrappedType.IsClass && typeof(Object).IsAssignableFrom(cache.WrappedType);

            if (canNotAssignUnityObject && !cache.IsVRefProp.boolValue)
            {
                cache.IsVRefProp.boolValue = true;
                if (SaintsInterfaceDrawer.SyncInterfaceModeSideEffectsWithoutApply(cache.ValueProp, cache.VRefProp,
                        true))
                {
                    cache.IsVRefProp.serializedObject.ApplyModifiedProperties();
                    RefreshFieldCache(cache, property, info, parent);
                }
            }

            Rect lineRect = new Rect(position)
            {
                height = SingleLineHeight,
            };
            Rect contentRect = EditorGUI.PrefixLabel(lineRect, label);
            Rect labelRect = new Rect(lineRect)
            {
                width = lineRect.width - contentRect.width,
            };
            DrawOverrideRichText(labelRect, label, overrideRichTextChunks);

            bool showModeButton = !canNotAssignUnityObject && !canOnlyAssignUnityObject;
            Rect activeRect = contentRect;
            if (showModeButton)
            {
                Rect modeRect = new Rect(contentRect)
                {
                    width = ModeButtonWidth,
                };

                if (GUI.Button(modeRect, GetModeContent(cache.IsVRefProp.boolValue), ModeButtonStyle))
                {
                    bool nextIsVRef = !cache.IsVRefProp.boolValue;
                    bool changed = cache.IsVRefProp.boolValue != nextIsVRef;
                    cache.IsVRefProp.boolValue = nextIsVRef;
                    changed |= SaintsInterfaceDrawer.SyncInterfaceModeSideEffectsWithoutApply(cache.ValueProp,
                        cache.VRefProp, nextIsVRef);
                    if (changed)
                    {
                        cache.IsVRefProp.serializedObject.ApplyModifiedProperties();
                        RefreshFieldCache(cache, property, info, parent);
                        TriggerChangedIMGUI(property, GetCurrentValue(cache));
                    }
                }

                activeRect.xMin = modeRect.xMax + FieldSpacing;
            }

            if (cache.IsVRefProp.boolValue)
            {
                DrawReferenceField(position, activeRect, property, info, parent, cache);
                return;
            }

            DrawObjectField(activeRect, property, info, parent, cache);
        }

        private void RefreshFieldCache(FieldStatusIMGUI cache, SerializedProperty property, FieldInfo info,
            object parent)
        {
            cache.Error = "";
            cache.WrappedType = GetWrappedValueType(property, info);
            if (cache.WrappedType == null)
            {
                cache.Error = $"Failed to resolve wrapped type for {property.propertyPath}";
                SaintsRowAttributeDrawer.ClearManagedReferenceBody(cache.BodyInfo);
                return;
            }

            cache.ValueFieldProp = property.FindPropertyRelative("valueField") ??
                                   SerializedUtils.FindPropertyByAutoPropertyName(property, "valueField");
            if (cache.ValueFieldProp == null)
            {
                cache.Error = $"valueField not found in {property.propertyPath}";
                SaintsRowAttributeDrawer.ClearManagedReferenceBody(cache.BodyInfo);
                return;
            }

            cache.ValueProp = cache.ValueFieldProp.FindPropertyRelative(nameof(SaintsSerializedProperty.V)) ??
                              SerializedUtils.FindPropertyByAutoPropertyName(cache.ValueFieldProp,
                                  nameof(SaintsSerializedProperty.V));
            cache.IsVRefProp = cache.ValueFieldProp.FindPropertyRelative(nameof(SaintsSerializedProperty.IsVRef)) ??
                               SerializedUtils.FindPropertyByAutoPropertyName(cache.ValueFieldProp,
                                   nameof(SaintsSerializedProperty.IsVRef));
            cache.VRefProp = cache.ValueFieldProp.FindPropertyRelative(nameof(SaintsSerializedProperty.VRef)) ??
                             SerializedUtils.FindPropertyByAutoPropertyName(cache.ValueFieldProp,
                                 nameof(SaintsSerializedProperty.VRef));

            if (cache.ValueProp == null || cache.IsVRefProp == null || cache.VRefProp == null)
            {
                cache.Error = $"Failed to resolve wrap body for {property.propertyPath}";
                SaintsRowAttributeDrawer.ClearManagedReferenceBody(cache.BodyInfo);
                return;
            }

            EnsureManagedReferenceWatcher(cache);
            EnsureExpandState(cache, info);

            cache.ReferenceLabel = Util.GetReferencePropertyLabel(cache.VRefProp);
            cache.ReferenceMetaInfo = GetReferenceMetaInfo(cache.VRefProp,
                SaintsInterfaceDrawer.GetTypesImplementingInterface(cache.WrappedType));

            SaintsRowAttributeDrawer.SyncManagedReferenceBody(cache.BodyInfo, cache.VRefProp, info, parent,
                cache.VRefProp.managedReferenceValue, this);
        }

        private void EnsureManagedReferenceWatcher(FieldStatusIMGUI cache)
        {
            if (cache.ManagedReferenceWatcher != null || cache.VRefProp == null)
            {
                return;
            }

            cache.ManagedReferenceWatcher = _ => cache.PendingManagedReferenceChanged = true;
            WatchChangedIMGUI(cache.VRefProp, cache.ManagedReferenceWatcher, true);
        }

        private void EnsureExpandState(FieldStatusIMGUI cache, MemberInfo info)
        {
            if (cache.ExpandStateInitialized || cache.VRefProp == null)
            {
                return;
            }

            IReadOnlyList<Attribute> allAttributes = OverrideAttributes ?? ReflectCache.GetCustomAttributes<Attribute>(info);
            cache.VRefProp.isExpanded =
                SaintsInterfaceDrawer.ShouldReferenceStartExpanded(allAttributes, cache.VRefProp);
            cache.ExpandStateInitialized = true;
        }

        private void DrawObjectField(Rect position, SerializedProperty property, FieldInfo info, object parent,
            FieldStatusIMGUI cache)
        {
            Object oldValue = cache.ValueProp.objectReferenceValue;
            EditorGUI.BeginChangeCheck();
            Object newValue = EditorGUI.ObjectField(position, GUIContent.none, oldValue, typeof(Object), true);
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            if (!SaintsInterfaceDrawer.TryGetMatchedInterfaceValue(newValue, typeof(Object), cache.WrappedType,
                    out Object matchedValue))
            {
                matchedValue = oldValue;
            }

            if (cache.ValueProp.objectReferenceValue == matchedValue)
            {
                return;
            }

            cache.ValueProp.objectReferenceValue = matchedValue;
            cache.ValueProp.serializedObject.ApplyModifiedProperties();
            RefreshFieldCache(cache, property, info, parent);
            TriggerChangedIMGUI(property, matchedValue);
        }

        private void DrawReferenceField(Rect position, Rect lineContentRect, SerializedProperty property, FieldInfo info,
            object parent, FieldStatusIMGUI cache)
        {
            Rect foldoutRect = new Rect(lineContentRect)
            {
                width = FoldoutWidth,
            };
            bool expanded = GUI.Toggle(foldoutRect, cache.VRefProp.isExpanded, GUIContent.none, EditorStyles.foldout);
            if (expanded != cache.VRefProp.isExpanded)
            {
                cache.VRefProp.isExpanded = expanded;
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
                        object currentValue = cache.VRefProp.managedReferenceValue;
                        object instance = curItem == null
                            ? null
                            : ReferencePickerAttributeDrawer.CopyObj(currentValue,
                                Activator.CreateInstance((Type)curItem));

                        cache.VRefProp.managedReferenceValue = instance;
                        cache.VRefProp.serializedObject.ApplyModifiedProperties();
                        RefreshFieldCache(cache, property, info, parent);
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

            if (!cache.VRefProp.isExpanded)
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
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                SaintsRowAttributeDrawer.DrawManagedReferenceBody(childRect, cache.BodyInfo);
                cache.PendingManagedReferenceChanged |= changed.changed;
            }

            if (!cache.PendingManagedReferenceChanged)
            {
                return;
            }

            cache.PendingManagedReferenceChanged = false;
            TriggerChangedIMGUI(property, cache.VRefProp.managedReferenceValue);
        }

        private static WrapType GetWrapMode(SerializedProperty property)
        {
            SerializedProperty wrapTypeProp = property.FindPropertyRelative("wrapType");
            return wrapTypeProp == null ? WrapType.T : (WrapType)wrapTypeProp.intValue;
        }

        private static GUIContent GetModeContent(bool isVRef) => new GUIContent(
            isVRef ? "<color=#ffa500>R</color>" : "<color=#00ffff>I</color>",
            isVRef ? "Serializable Reference" : "Unity Instance");

        private static object GetCurrentValue(FieldStatusIMGUI cache) => cache.IsVRefProp.boolValue
            ? cache.VRefProp.managedReferenceValue
            : cache.ValueProp.objectReferenceValue;

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
    }
}
