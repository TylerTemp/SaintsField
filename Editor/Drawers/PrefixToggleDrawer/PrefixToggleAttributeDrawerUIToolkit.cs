#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PrefixToggleDrawer
{
    public partial class PrefixToggleAttributeDrawer
    {
        private static string NameToggle(SerializedProperty property, int index) => $"{SerializedUtils.GetUniqueId(property)}_{index}__PrefixToggle";
        private static string NameToggleError(SerializedProperty property, int index) => $"{SerializedUtils.GetUniqueId(property)}_{index}__PrefixToggle_HelpBox";

        protected override VisualElement CreatePreFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<Attribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            // skip list element as myIs.xxx.xxx.Array.data[INDEX]
            if (SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0)
            {
                return null;
            }

            Toggle result = new Toggle
            {
                style =
                {
                    marginRight = 2,
                    flexGrow = 0,
                    flexShrink = 0,
                },
                name = NameToggle(property, index),
            };
            result.AddToClassList(ClassAllowDisable);

            return result;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<Attribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
                },
                name = NameToggleError(property, index),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<Attribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            // Debug.Log(property.propertyPath);

            HelpBox helpBox = container.Q<HelpBox>(NameToggleError(property, index));
            Toggle toggle = container.Q<Toggle>(NameToggle(property, index));

            PrefixToggleAttribute attr = (PrefixToggleAttribute)saintsAttribute;
            string targetStr = attr.TargetName;
            string showIf = attr.ShowIf;

            (string error, SerializedProperty toggleProperty) = PrefixToggleUtils.GetToggleTarget(targetStr, property);
            if (error != "")
            {
                UIToolkitUtils.SetHelpBox(helpBox, error);
                toggle.SetEnabled(false);
                return;
            }

            if (toggleProperty != null)
            {
                toggle.BindProperty(toggleProperty);

                // ReSharper disable once InvertIf
                if (!string.IsNullOrEmpty(showIf))
                {
                    RefreshShowIf();
                    toggle.schedule.Execute(RefreshShowIf).Every(150);
                }
                return;
            }

            toggle.RegisterValueChangedCallback(evt =>
            {
                bool on = evt.newValue;
                object userData = toggle.userData;
                if (userData == null)
                {
                    RefreshToggleNonSer();
                    return;
                }

                Payload payload = (Payload)userData;

                try
                {
                    payload.SetValue(on);
                }
                catch (Exception e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                    RefreshToggleNonSer();
                    UIToolkitUtils.SetHelpBox(helpBox, e.InnerException?.Message ?? e.Message);
                }
            });

            RefreshToggleNonSer();
            toggle.schedule.Execute(RefreshToggleNonSer).Every(150);
            return;

            void RefreshShowIf()
            {
                if (!SerializedUtils.IsOk(property))
                {
                    return;
                }

                object p = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                RefreshShowIfInternal(p);
            }

            bool RefreshShowIfInternal(object p)
            {
                if (string.IsNullOrEmpty(showIf))
                {
                    return true;
                }

                (string showError, MemberInfo _, object showResult) = Util.GetOf<object>(showIf, null, property, info, p, null);
                if (showError != "")
                {
                    UIToolkitUtils.SetHelpBox(helpBox, showError);
                    return false;
                }

                bool show = ReflectUtils.Truly(showResult);
                DisplayStyle showType = show
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                UIToolkitUtils.SetDisplayStyle(toggle, showType);
                return show;

            }

            void RefreshToggleNonSer()
            {
                if (!SerializedUtils.IsOk(property))
                {
                    return;
                }

                object p = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

                if (!RefreshShowIfInternal(p))
                {
                    return;
                }

                (string refError, MemberInfo memberInfo, object result) = Util.GetOf<object>(targetStr, null, property, info, p, null);
                UIToolkitUtils.SetHelpBox(helpBox, refError);
                if (refError != "")
                {
                    toggle.SetEnabled(false);
                    return;
                }

                if (!toggle.enabledSelf)
                {
                    toggle.SetEnabled(true);
                }

                toggle.userData = new Payload(memberInfo, p);
                toggle.SetValueWithoutNotify(ReflectUtils.Truly(result));
            }
        }
    }
}
#endif
