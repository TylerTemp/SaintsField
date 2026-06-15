#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ReferencePicker
{
    public partial class ReferencePickerAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public string DisplayLabel = "";
            public object ManagedReferenceValue;
            public AdvancedDropdownMetaInfo MetaInfo;
            public SaintsRowAttributeDrawer.ManagedReferenceBodyInfo BodyInfo =
                new SaintsRowAttributeDrawer.ManagedReferenceBodyInfo();
            public bool ExpandStateInitialized;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private static void ClearCacheInfo(InfoIMGUI cache)
        {
            SaintsRowAttributeDrawer.ClearManagedReferenceBody(cache.BodyInfo);
            cache.ManagedReferenceValue = null;
            cache.MetaInfo = default;
            cache.DisplayLabel = "";
            cache.Error = "";
            cache.ExpandStateInitialized = false;
        }

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                ClearCacheInfo(cache);
                InfoCacheIMGUI.Remove(key);
            });
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            InfoIMGUI cache = EnsureKey(property);
            try
            {
                RefreshCache(cache, property, info, parent);
            }
            catch (InvalidOperationException e)
            {
                cache.Error = e.Message;
            }

            float height = EditorGUIUtility.singleLineHeight;
            if (cache.Error != "")
            {
                return height;
            }

            if (cache.ManagedReferenceValue == null || !property.isExpanded)
            {
                return height;
            }

            return height + SaintsRowAttributeDrawer.GetManagedReferenceBodyHeight(cache.BodyInfo, width - IndentWidth);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cache = EnsureKey(property);
            try
            {
                RefreshCache(cache, property, info, parent);
            }
            catch (InvalidOperationException e)
            {
                cache.Error = e.Message;
                return;
            }

            bool hideLabel = ((ReferencePickerAttribute)saintsAttribute).HideLabel;
            Rect lineRect = new Rect(position)
            {
                height = SingleLineHeight,
            };
            Rect labelRect = new Rect();

            if (cache.ManagedReferenceValue != null)
            {
                if (!cache.ExpandStateInitialized)
                {
                    property.isExpanded = allAttributes.Any(each => each is FieldDefaultExpandAttribute) || property.isExpanded;
                    cache.ExpandStateInitialized = true;
                }

                Rect foldoutRect = new Rect(lineRect)
                {
                    width = 12f,
                };
                property.isExpanded = GUI.Toggle(foldoutRect, property.isExpanded, GUIContent.none, EditorStyles.foldout);
                lineRect.xMin = foldoutRect.xMax + 2f;
            }

            Rect fieldRect;
            if (hideLabel)
            {
                fieldRect = lineRect;
            }
            else
            {
                labelRect = new Rect(lineRect)
                {
                    width = Mathf.Min(EditorGUIUtility.labelWidth, lineRect.width),
                };
                fieldRect = new Rect(lineRect)
                {
                    xMin = labelRect.xMax,
                };
            }

            if (!hideLabel && cache.ManagedReferenceValue != null && GUI.Button(labelRect, label, EditorStyles.label))
            {
                property.isExpanded = !property.isExpanded;
            }
            else if (!hideLabel)
            {
                EditorGUI.LabelField(labelRect, label);
            }

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                    cache.MetaInfo,
                    Mathf.Max(fieldRect.width, 220f),
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        object instance = curItem == null
                            ? null
                            : CopyObj(cache.ManagedReferenceValue, Activator.CreateInstance((Type)curItem));

                        property.managedReferenceValue = instance;
                        property.serializedObject.ApplyModifiedProperties();
                        TriggerChangedIMGUI(property, instance);

                        try
                        {
                            RefreshCache(cache, property, info, parent);
                        }
                        catch (InvalidOperationException e)
                        {
                            cache.Error = e.Message;
                        }

                        return null;
                    }));
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect,
                RichTextDrawer.ParseRichXmlWithProvider(cache.DisplayLabel, new RichTextDrawer.EmptyRichTextTagProvider()));

            if (cache.Error != "" || cache.ManagedReferenceValue == null || !property.isExpanded)
            {
                return;
            }

            Rect childRect = new Rect(position)
            {
                x = position.x + IndentWidth,
                y = lineRect.yMax,
                width = position.width - IndentWidth,
                height = position.height - SingleLineHeight,
            };
            SaintsRowAttributeDrawer.DrawManagedReferenceBody(childRect, cache.BodyInfo);
        }

        private void RefreshCache(InfoIMGUI cache, SerializedProperty property, FieldInfo info, object parent)
        {
            object managedReferenceValue = property.managedReferenceValue;
            cache.ManagedReferenceValue = managedReferenceValue;
            cache.DisplayLabel = Util.GetReferencePropertyLabel(property);

            Dropdown<Type> dropdownList = new Dropdown<Type>
            {
                { "[Null]", null },
            };

            Dictionary<string, List<Type>> nameSpaceToTypes = new Dictionary<string, List<Type>>();
            foreach (Type type in GetTypes(property))
            {
                string typeNamespace = type.Namespace ?? "";
                if (!nameSpaceToTypes.TryGetValue(typeNamespace, out List<Type> list))
                {
                    nameSpaceToTypes[typeNamespace] = list = new List<Type>();
                }

                list.Add(type);
            }

            foreach (string @namespace in nameSpaceToTypes.Keys.OrderBy(each => each))
            {
                Dropdown<Type> namespaceTypes = new Dropdown<Type>(@namespace == "" ? "[No Namespace]" : @namespace);
                foreach (Type eachType in nameSpaceToTypes[@namespace])
                {
                    namespaceTypes.Add(eachType.Name, eachType);
                }

                dropdownList.Add(namespaceTypes);
            }

            cache.MetaInfo = new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurDisplay = managedReferenceValue == null ? "-" : managedReferenceValue.GetType().Name,
                CurValues = managedReferenceValue == null ? Array.Empty<object>() : new object[] { managedReferenceValue.GetType() },
                DropdownListValue = dropdownList,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            if (managedReferenceValue != null)
            {
                long lastManagedReferenceId = cache.BodyInfo.ManagedReferenceId;
                SaintsRowAttributeDrawer.SyncManagedReferenceBody(cache.BodyInfo, property, info, parent,
                    managedReferenceValue, this);
                if (lastManagedReferenceId != cache.BodyInfo.ManagedReferenceId)
                {
                    cache.ExpandStateInitialized = false;
                }
            }

            cache.Error = "";
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
            string error = EnsureKey(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
#endif
