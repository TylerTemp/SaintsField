#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Utils
{
    public static class UIToolkitCache
    {
#if UNITY_2021_3_OR_NEWER
        // public static Type GetDecDrawer(PropertyAttribute propertyAttribute)
        // {
        //     var type = SaintsPropertyDrawer.PropertyGetDecoratorDrawer(propertyAttribute.GetType());
        // }

        public static VisualElement CreateDec(PropertyAttribute propAttribute, Type drawerType)
        {
            DecoratorDrawer decorator = (DecoratorDrawer)Activator.CreateInstance(drawerType);
            FieldInfo mAttributeField = drawerType.GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance);
            if (mAttributeField != null)
            {
                mAttributeField.SetValue(decorator, propAttribute);
            }

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            root.AddToClassList("unity-decorator-drawers-container");

            VisualElement ve =
#if UNITY_2022_3_OR_NEWER
                    decorator.CreatePropertyGUI()
#else
                    null
#endif
                ;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (ve == null)
            {
                ve = new IMGUIContainer(() =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    // float styleWidth = root.resolvedStyle.width;
                    // float width = double.IsNaN(styleWidth)? Screen.width: styleWidth;

                    Rect position = new Rect
                    {
                        height = decorator.GetHeight(),
                        // width = width,
                    };
                    decorator.OnGUI(position);
                    // ReSharper disable once PossibleNullReferenceException
                    // ReSharper disable once AccessToModifiedClosure
                    ve.style.height = position.height;
                });
                root.style.height = decorator.GetHeight();
            }

            root.Add(ve);

            return root;
        }

        public static VisualElement MergeWithDec(VisualElement result, IReadOnlyList<PropertyAttribute> allAttributes)
        {
            VisualElement dec = DrawDecorator(allAttributes);
            if(dec == null)
            {
                return result;
            }

            VisualElement container = new VisualElement();
            container.Add(dec);
            container.Add(result);
            return container;
        }


        private static VisualElement DrawDecorator(IEnumerable<PropertyAttribute> allPropAttributes)
        {
            // this can be multiple, should not be a dictionary
            IEnumerable<(PropertyAttribute propAttribute, Type)> decDrawers =
                allPropAttributes
                    .Select(propAttribute => (propAttribute, SaintsPropertyDrawer.PropertyGetDecoratorDrawer(propAttribute.GetType())))
                    .Where(each => each.Item2 != null);
            // new List<(PropertyAttribute, Type drawerType)>();
            // if (decDrawers.Length == 0)
            // {
            //     return null;
            // }

            VisualElement result = new VisualElement
            {
                name = "saints-field-drawers-container",
            };
            result.AddToClassList("unity-decorator-drawers-container");
            // this.m_DecoratorDrawersContainer = new VisualElement();
            // this.m_DecoratorDrawersContainer.AddToClassList(PropertyField.decoratorDrawersContainerClassName);

            bool hasAny = false;
            foreach ((PropertyAttribute propAttribute, Type drawerType) in decDrawers)
            {
                hasAny = true;
                DecoratorDrawer decorator = (DecoratorDrawer)Activator.CreateInstance(drawerType);
                FieldInfo mAttributeField = drawerType.GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mAttributeField != null)
                {
                    mAttributeField.SetValue(decorator, propAttribute);
                }

                VisualElement ve =
#if UNITY_2022_3_OR_NEWER
                        decorator.CreatePropertyGUI()
#else
                        null
#endif
                    ;

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (ve == null)
                {
                    ve = new IMGUIContainer(() =>
                    {
                        Rect position = new Rect
                        {
                            height = decorator.GetHeight(),
                            width = result.resolvedStyle.width,
                        };
                        decorator.OnGUI(position);
                        // ReSharper disable once PossibleNullReferenceException
                        // ReSharper disable once AccessToModifiedClosure
                        ve.style.height = position.height;
                    });
                    ve.style.height = decorator.GetHeight();
                }
                result.Add(ve);
            }

            return hasAny? result: null;
        }
#endif
    }
}
