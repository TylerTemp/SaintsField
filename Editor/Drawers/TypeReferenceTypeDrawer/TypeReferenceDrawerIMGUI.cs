using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.TypeReferenceTypeDrawer
{
    public partial class TypeReferenceDrawer
    {
        protected override bool UseCreateFieldIMGUI => true;

        private sealed class TypeReferenceStatusIMGUI
        {
            public string ContextError = "";
            public string SelectionError = "";
            public TypeReferenceContext Context;
            public IReadOnlyList<Assembly> CachedAssemblies;
            public readonly Dictionary<Assembly, Type[]> CachedAssembliesTypes = new Dictionary<Assembly, Type[]>();
            public AdvancedDropdownMetaInfo MetaInfo;
            public string DefaultSearch;
        }

        private static readonly Dictionary<string, TypeReferenceStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, TypeReferenceStatusIMGUI>();

        private static TypeReferenceStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out TypeReferenceStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new TypeReferenceStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, int index,
            ISaintsAttribute saintsAttribute,
            FieldInfo info, bool hasLabelWidth, object parent)
        {
            TypeReferenceStatusIMGUI cache = RefreshCache(property, allAttributes: null, parent: parent, updateDropdown: false);
            return cache.ContextError == ""
                ? EditorGUIUtility.singleLineHeight
                : ImGuiHelpBox.GetHeight(cache.ContextError, width, EMessageType.Error);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            TypeReferenceStatusIMGUI cache = RefreshCache(property, allAttributes, parent, true);
            if (cache.ContextError != "")
            {
                ImGuiHelpBox.Draw(position, cache.ContextError, EMessageType.Error);
                return;
            }

            Rect leftRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            string display = cache.MetaInfo.CurDisplay;

            if (EditorGUI.DropdownButton(leftRect, new GUIContent(display), FocusType.Keyboard))
            {
                SaintsTreeDropdownIMGUI dropdown = new SaintsTreeDropdownIMGUI(
                    cache.MetaInfo,
                    Mathf.Max(leftRect.width, 220f),
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        TypeReference changedValue = SetValue(cache.Context, curItem as Type);
                        TriggerChangedIMGUI(property, changedValue);
                        return null;
                    });

                if (!string.IsNullOrEmpty(cache.DefaultSearch))
                {
                    dropdown.SetSearch(cache.DefaultSearch);
                }

                PopupWindow.Show(leftRect, dropdown);
            }
        }

        private static TypeReferenceStatusIMGUI RefreshCache(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, object parent, bool updateDropdown)
        {
            TypeReferenceStatusIMGUI cache = EnsureKey(property);
            (string contextError, TypeReferenceContext context) = GetTypeReferenceContext(property);
            cache.ContextError = contextError;
            cache.Context = context;
            cache.SelectionError = "";
            if (contextError != "")
            {
                return cache;
            }

            (string selectionError, Type type) = GetSelectedType(context);
            cache.SelectionError = selectionError;
            if (!updateDropdown)
            {
                return cache;
            }

            TypeReferenceAttribute typeReferenceAttribute = GetTypeReferenceAttribute(allAttributes);
            cache.DefaultSearch = typeReferenceAttribute?.DefaultSearch;
            cache.CachedAssemblies ??= GetAssembly(typeReferenceAttribute, parent).ToArray();
            FillAssembliesTypes(cache.CachedAssemblies, cache.CachedAssembliesTypes);
            cache.MetaInfo = GetDropdownMetaInfo(type, typeReferenceAttribute, cache.CachedAssemblies,
                cache.CachedAssembliesTypes, true, parent);
            return cache;
        }

        protected override bool WillDrawBelow(SerializedProperty property, IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            TypeReferenceStatusIMGUI cache = RefreshCache(property, allAttributes, parent, false);
            return cache.ContextError == "" && !string.IsNullOrEmpty(cache.SelectionError);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            TypeReferenceStatusIMGUI cache = RefreshCache(property, allAttributes, parent, false);
            return string.IsNullOrEmpty(cache.SelectionError) || cache.ContextError != ""
                ? 0
                : ImGuiHelpBox.GetHeight(cache.SelectionError, width, EMessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            TypeReferenceStatusIMGUI cache = RefreshCache(property, allAttributes, parent, false);
            return string.IsNullOrEmpty(cache.SelectionError) || cache.ContextError != ""
                ? position
                : ImGuiHelpBox.Draw(position, cache.SelectionError, EMessageType.Error);
        }
    }
}
