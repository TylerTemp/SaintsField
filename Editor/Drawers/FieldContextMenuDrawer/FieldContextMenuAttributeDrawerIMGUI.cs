using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.FieldContextMenuDrawer
{
    public partial class FieldContextMenuAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public string ExecError = "";
            public readonly HashSet<IEnumerator> Enumerators = new HashSet<IEnumerator>();
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property, int index)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{index}";
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            EditorApplication.contextualPropertyMenu += OnContextualPropertyMenu;
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                EditorApplication.contextualPropertyMenu -= OnContextualPropertyMenu;
                InfoCacheIMGUI.Remove(key);
            });
            return cache;
        }

        protected override void OnPropertyEndImGui(Rect labelFieldRect, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int saintsIndex, FieldInfo info, object parent)
        {
            TickEnumerators(property, saintsIndex);
        }

        private static void TickEnumerators(SerializedProperty property, int index)
        {
            InfoIMGUI cache = EnsureKey(property, index);
            if (cache.Enumerators.Count == 0)
            {
                return;
            }

            HashSet<IEnumerator> completed = new HashSet<IEnumerator>();
            foreach (IEnumerator enumerator in cache.Enumerators)
            {
                if (!enumerator.MoveNext())
                {
                    completed.Add(enumerator);
                }
            }

            cache.Enumerators.ExceptWith(completed);
        }

        private static void OnContextualPropertyMenu(GenericMenu menu, SerializedProperty property)
        {
            (PropertyAttribute[] allAttributes, object parent) =
                SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(property);
            if (parent == null)
            {
                return;
            }

            SerializedUtils.FieldOrProp fieldOrProp = SerializedUtils.GetFieldInfoAndDirectParent(property).fieldOrProp;
            if (!fieldOrProp.IsField || fieldOrProp.FieldInfo == null)
            {
                return;
            }

            FieldInfo info = fieldOrProp.FieldInfo;
            bool addedAny = false;

            foreach ((FieldCustomContextMenuAttribute attribute, int saintsIndex) in allAttributes
                         .OfType<ISaintsAttribute>()
                         .Select((each, saintsIndex) => (each, saintsIndex))
                         .Where(each => each.each is FieldCustomContextMenuAttribute)
                         .Select(each => ((FieldCustomContextMenuAttribute)each.each, each.saintsIndex)))
            {
                InfoIMGUI cache = EnsureKey(property, saintsIndex);
                cache.Error = "";

                string callback = attribute.FuncName;
                string menuName = string.IsNullOrEmpty(attribute.MenuName)
                    ? ObjectNames.NicifyVariableName(callback)
                    : attribute.MenuName;

                string useMenuName = menuName;
                bool isChecked = false;
                bool isDisabled = false;

                if (attribute.MenuNameIsCallback)
                {
                    (string error, MemberInfo _, object result) =
                        Util.GetOf<object>(menuName, null, property, info, GetParent(property, parent), null);
                    if (error != "")
                    {
                        cache.Error = error;
                        continue;
                    }

                    if (result is null or "")
                    {
                        continue;
                    }

                    if (result is ValueTuple<string, EContextMenuStatus> statusTuple)
                    {
                        useMenuName = statusTuple.Item1;
                        if (useMenuName is null or "")
                        {
                            continue;
                        }

                        isChecked = statusTuple.Item2 == EContextMenuStatus.Checked;
                        isDisabled = statusTuple.Item2 == EContextMenuStatus.Disabled;
                    }
                    else
                    {
                        useMenuName = result.ToString();
                    }
                }

                if (!addedAny)
                {
                    menu.AddSeparator("");
                    addedAny = true;
                }

                if (callback == null)
                {
                    menu.AddSeparator(useMenuName);
                    continue;
                }

                if (isDisabled)
                {
                    menu.AddDisabledItem(new GUIContent(useMenuName), isChecked);
                    continue;
                }

                menu.AddItem(new GUIContent(useMenuName), isChecked, () =>
                {
                    cache.ExecError = "";
                    cache.Enumerators.Clear();

                    foreach ((string eachError, MemberInfo _, object buttonResult) in
                             DecButtonAttributeDrawer.CallButtonFunc(property, callback, info, GetParent(property, parent)))
                    {
                        if (eachError == "")
                        {
                            if (buttonResult is IEnumerator enumerator)
                            {
                                cache.Enumerators.Add(enumerator);
                            }
                        }
                        else
                        {
                            cache.ExecError += eachError;
                        }
                    }
                });
            }
        }

        private static object GetParent(SerializedProperty property, object parent)
        {
            if (parent != null && ReflectUtils.TypeIsStruct(parent.GetType()))
            {
                (SerializedUtils.FieldOrProp _, object refreshedParent) =
                    SerializedUtils.GetFieldInfoAndDirectParent(property);
                if (refreshedParent != null)
                {
                    return refreshedParent;
                }
            }

            return parent;
        }

        private static string GetDisplayError(SerializedProperty property, int index)
        {
            InfoIMGUI cache = EnsureKey(property, index);
            if (cache.Error != "" && cache.ExecError != "")
            {
                return $"{cache.Error}\n\n{cache.ExecError}";
            }

            return $"{cache.Error}{cache.ExecError}";
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent) => GetDisplayError(property, index) != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            string error = GetDisplayError(property, index);
            return error == "" ? 0f : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = GetDisplayError(property, index);
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
