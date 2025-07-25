using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.TypeReferenceTypeDrawer
{
    public partial class TypeReferenceDrawer
    {
        protected override bool UseCreateFieldIMGUI => true;

        private class CachedImGui
        {
            public string Error = "";

            public bool Changed;
            public TypeReference ChangedValue;

            public IReadOnlyList<Assembly> CachedAsssemblies;
            public readonly Dictionary<Assembly, Type[]> CachedAsssembliesTypes = new Dictionary<Assembly, Type[]>();
        }

        private static readonly Dictionary<string, CachedImGui> CachedImGuiDict = new Dictionary<string, CachedImGui>();

        private static CachedImGui EnsureCache(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once InvertIf
            if(!CachedImGuiDict.TryGetValue(key, out CachedImGui cachedImGui))
            {
                CachedImGuiDict[key] = cachedImGui = new CachedImGui();
                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    CachedImGuiDict.Remove(key);
                });
            }

            return cachedImGui;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            CachedImGui cache = EnsureCache(property);
            if (cache.Changed)
            {
                cache.Changed = false;
                onGUIPayload.SetValue(cache.ChangedValue);
            }

            TypeReferenceAttribute typeReferenceAttribute = GetTypeReferenceAttribute(allAttributes);

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (cache.CachedAsssemblies == null)
            {
                cache.CachedAsssemblies = GetAssembly(typeReferenceAttribute, parent).ToArray();
            }
            FillAsssembliesTypes(cache.CachedAsssemblies, cache.CachedAsssembliesTypes);

            (string error, Type type) = GetSelectedType(property);
            cache.Error = error;

            AdvancedDropdownMetaInfo metaInfo = GetDropdownMetaInfo(type, typeReferenceAttribute, cache.CachedAsssemblies, cache.CachedAsssembliesTypes, true, parent);

            #region Dropdown

            Rect leftRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            string display = type == null? "null": FormatName(type, true);

            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(display), FocusType.Keyboard))
            {
                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(metaInfo.DropdownListValue, position.width);

                // OnGUIPayload targetPayload = onGUIPayload;
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    metaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        TypeReference r = SetValue(property, curItem as Type);
                        cache.Changed = true;
                        cache.ChangedValue = r;
                        // Debug.Log($"Advanced Changed: {AsyncChangedCache[key].changed}/{AsyncChangedCache[key].GetHashCode()}");
                        // if(ExpandableIMGUIScoop.IsInScoop)
                        // {
                        //     property.serializedObject.ApplyModifiedProperties();
                        // }
                    },
                    _ => null);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }

            #endregion
        }

        protected override bool WillDrawBelow(SerializedProperty property, IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            CachedImGui cache = EnsureCache(property);
            return !string.IsNullOrEmpty(cache.Error);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            CachedImGui cache = EnsureCache(property);
            return string.IsNullOrEmpty(cache.Error)
                ? 0
                : ImGuiHelpBox.GetHeight(cache.Error, width, EMessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            CachedImGui cache = EnsureCache(property);
            return string.IsNullOrEmpty(cache.Error)
                ? position
                : ImGuiHelpBox.Draw(position, cache.Error, EMessageType.Error);
        }
    }
}
