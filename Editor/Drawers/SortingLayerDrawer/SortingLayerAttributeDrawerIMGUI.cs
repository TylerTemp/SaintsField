using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public partial class SortingLayerAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public bool Changed;
            public object ChangedValue;
            public string Display = "";
        }

        private enum SortingLayerItemType
        {
            None,
            Normal,
            EmptyString,
            OpenEditor,
        }

        private readonly struct SortingLayerPayload : IEquatable<SortingLayerPayload>
        {
            public readonly string Name;
            public readonly int Id;
            public readonly SortingLayerItemType Type;

            public SortingLayerPayload(int id, string name)
            {
                Name = name;
                Id = id;
                Type = SortingLayerItemType.Normal;
            }

            public SortingLayerPayload(SortingLayerItemType type, string name)
            {
                Name = name;
                Id = int.MinValue;
                Type = type;
            }

            public bool Equals(SortingLayerPayload other)
            {
                return Id == other.Id && Type == other.Type;
            }

            public override bool Equals(object obj)
            {
                return obj is SortingLayerPayload other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, (int)Type);
            }
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Integer &&
                property.propertyType != SerializedPropertyType.String)
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            InfoIMGUI cache = EnsureKey(property);
            if (cache.Changed)
            {
                cache.Changed = false;
                TriggerChangedIMGUI(property, cache.ChangedValue);
            }

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                    GetMetaInfo(property, out cache.Display),
                    fieldRect.width,
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        SortingLayerPayload payload = (SortingLayerPayload)curItem;
                        switch (payload.Type)
                        {
                            case SortingLayerItemType.Normal:
                            {
                                object changedValue;
                                if (property.propertyType == SerializedPropertyType.String)
                                {
                                    property.stringValue = payload.Name;
                                    changedValue = payload.Name;
                                }
                                else
                                {
                                    property.intValue = payload.Id;
                                    changedValue = payload.Id;
                                }

                                property.serializedObject.ApplyModifiedProperties();
                                cache.Changed = true;
                                cache.ChangedValue = changedValue;
                                cache.Display = GetDisplay(property);
                                return null;
                            }
                            case SortingLayerItemType.EmptyString:
                                property.stringValue = "";
                                property.serializedObject.ApplyModifiedProperties();
                                cache.Changed = true;
                                cache.ChangedValue = "";
                                cache.Display = GetDisplay(property);
                                return null;
                            case SortingLayerItemType.OpenEditor:
                                SortingLayerUtils.OpenSortingLayerInspector();
                                return null;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }));
            }
            else
            {
                cache.Display = GetDisplay(property);
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect, RichTextDrawer.ParseRichXmlWithProvider(cache.Display, this));
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(SerializedProperty property, out string display)
        {
            AdvancedDropdownList<SortingLayerPayload> dropdown = new AdvancedDropdownList<SortingLayerPayload>();
            SortingLayerPayload? selected = null;

            if (property.propertyType == SerializedPropertyType.String)
            {
                SortingLayerPayload emptyItem = new SortingLayerPayload(SortingLayerItemType.EmptyString, "");
                dropdown.Add("[Empty String]", emptyItem);
                if (string.IsNullOrEmpty(property.stringValue))
                {
                    selected = emptyItem;
                }
            }

            if (SortingLayer.layers.Length > 0)
            {
                dropdown.AddSeparator();
                foreach (SortingLayer sortingLayer in SortingLayer.layers)
                {
                    SortingLayerPayload payload = new SortingLayerPayload(sortingLayer.id, sortingLayer.name);
                    dropdown.Add($"{sortingLayer.name} <color=#808080>({sortingLayer.id})</color>", payload);
                    if (property.propertyType == SerializedPropertyType.String && sortingLayer.name == property.stringValue
                        || property.propertyType == SerializedPropertyType.Integer && sortingLayer.id == property.intValue)
                    {
                        selected = payload;
                    }
                }

                dropdown.AddSeparator();
            }

            dropdown.Add("Edit Sorting Layers", new SortingLayerPayload(SortingLayerItemType.OpenEditor, null), false, "d_editicon.sml");

            display = GetDisplay(property);
            return new AdvancedDropdownMetaInfo
            {
                CurValues = selected.HasValue ? new object[] { selected.Value } : Array.Empty<object>(),
                DropdownListValue = dropdown,
            };
        }

        private static string GetDisplay(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                foreach (SortingLayer layer in SortingLayer.layers)
                {
                    if (layer.name == property.stringValue)
                    {
                        return $"{layer.name} <color=#808080>({layer.id})</color>";
                    }
                }

                return $"<color=red>?</color> {(string.IsNullOrEmpty(property.stringValue) ? "" : $"({property.stringValue})")}";
            }

            foreach (SortingLayer layer in SortingLayer.layers)
            {
                if (layer.id == property.intValue)
                {
                    return $"{layer.name} <color=#808080>({layer.id})</color>";
                }
            }

            return $"<color=red>?</color> ({property.intValue})";
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => property.propertyType != SerializedPropertyType.Integer &&
                              property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            property.propertyType != SerializedPropertyType.Integer &&
            property.propertyType != SerializedPropertyType.String
                ? ImGuiHelpBox.GetHeight($"Expect string or int, get {property.propertyType}", width, MessageType.Error)
                : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent) => ImGuiHelpBox.Draw(position,
            $"Expect string or int, get {property.propertyType}", MessageType.Error);
    }
}
