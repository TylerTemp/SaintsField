#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.ParticlePlayDrawer
{
    public partial class ParticlePlayAttributeDrawer
    {
        private static string NameContainer(SerializedProperty property) => $"{property.propertyPath}__ParticlePlay";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__ParticlePlay_HelpBox";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return new VisualElement();
            }
            (string _, ParticleSystem result) = GetParticleSystemOnObjectValue(property);
            ParticlePlayButtons element = new ParticlePlayButtons(result)
            {
                name = NameContainer(property),
            };
            element.AddToClassList(ClassAllowDisable);
            return element;
        }

        private static (string error, ParticleSystem result) GetParticleSystemOnObjectValue(SerializedProperty property)
        {
            Object obj = property.objectReferenceValue;
            if (RuntimeUtil.IsNull(obj))
            {
                return ("", null);
            }

            switch (obj)
            {
                case ParticleSystem particleSystem:
                    return ("", particleSystem);
                case Component component:
                    return ("", component.GetComponent<ParticleSystem>());
                case GameObject go:
                    return ("", go.GetComponent<ParticleSystem>());
                default:
                    return ($"Not supported type: {obj.GetType()}", null);
            }
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return new HelpBox($"type {property.propertyType} is not a ParticleSystem, GameObject, or Component", HelpBoxMessageType.Error);
            }

            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return;
            }

            HelpBox helpBox = container.Q<HelpBox>(name: NameHelpBox(property));
            ParticlePlayButtons particlePlayButtons = container.Q<ParticlePlayButtons>(name: NameContainer(property));
            particlePlayButtons.TrackPropertyValue(property, prop =>
            {
                (string error, ParticleSystem result) = GetParticleSystemOnObjectValue(prop);
                UIToolkitUtils.SetHelpBox(helpBox, error);

                particlePlayButtons.SetParticleSystem(result);
            });
        }
    }
}
#endif
