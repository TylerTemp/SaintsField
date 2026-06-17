#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
    public class SaintsInterfaceElement: VisualElement
    {
        private readonly SerializedProperty _valueProp;
        private readonly SerializedProperty _vRef;
        private readonly VisualElement _saintsRowElement;
        private readonly GeneralUObjectPicker _objectContainer;
        private readonly VisualElement _referenceContainer;


        public SaintsInterfaceElement(Type valueType, Type interfaceType, SerializedProperty property, SerializedProperty valueProp, IReadOnlyList<Attribute> allAttributes, FieldInfo info, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parentObj)
        {
            _valueProp = valueProp;
            SerializedProperty isVRef = property.FindPropertyRelative("IsVRef") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "IsVRef");
            _vRef = property.FindPropertyRelative("VRef") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "VRef");

            style.flexDirection = FlexDirection.Row;

            IsVRefButton isVRefButton = new IsVRefButton
            {
                bindingPath = isVRef.propertyPath,
            };
            Add(isVRefButton);
            isVRefButton.RegisterValueChangedCallback(v =>
            {
                UpdateVRefChange(v.newValue);
            });

            VisualElement columnContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            Add(columnContainer);

            _objectContainer =
                new GeneralUObjectPicker(valueProp.propertyPath, valueProp.objectReferenceValue, valueType, interfaceType, property.serializedObject.targetObject)
                    {
                        // bindingPath = valueProp.propertyPath,
                        // value = valueProp.objectReferenceValue,
                    };

            columnContainer.Add(_objectContainer);

            _referenceContainer = new VisualElement();
            columnContainer.Add(_referenceContainer);
            VisualElement referenceHContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            _referenceContainer.Add(referenceHContainer);

            // _dropdownIcon = Util.LoadResource<Texture2D>("classic-dropdown.png");
            // _dropdownRightIcon = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
            bool expand = SaintsInterfaceDrawer.ShouldReferenceStartExpanded(allAttributes, _vRef);

            ExpandButtonElement referenceExpandButton = new ExpandButtonElement
            {
                value = expand,
            };
            referenceExpandButton.SetViewDataKey(_vRef.propertyPath);
            referenceHContainer.Add(referenceExpandButton);

            UIToolkitUtils.DropdownButtonField dropdownBtn = UIToolkitUtils.ReferenceDropdownButtonField("",
                _vRef, this, () => SaintsInterfaceDrawer.GetTypesImplementingInterface(interfaceType));
            referenceHContainer.Add(dropdownBtn);
            dropdownBtn.style.marginLeft = 0;
            dropdownBtn.ButtonElement.style.borderTopLeftRadius = 0;
            dropdownBtn.ButtonElement.style.borderBottomLeftRadius = 0;
            dropdownBtn.labelElement.style.marginLeft = 0;
            dropdownBtn.TrackPropertyValue(_vRef, vr => dropdownBtn.ButtonLabelElement.text = Util.GetReferencePropertyLabel(vr));

            // var g = SerializedUtils.GetFileOrProp(parentObj, "VRef");
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;
            MemberInfo vPropInfo = fieldType.GetField("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (vPropInfo == null)
            {
                vPropInfo = fieldType.GetProperty("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }

            Debug.Assert(vPropInfo != null, fieldType);

            _saintsRowElement = SaintsRowAttributeDrawer.CreateElement(_vRef, "", vPropInfo,
                true, new SaintsRowAttribute(inline: true), makeRenderer, doTweenPlayRecorder, parentObj, new RichTextDrawer.EmptyRichTextTagProvider());
            _referenceContainer.Add(_saintsRowElement);

            UpdateExpand(referenceExpandButton.value);
            // _referenceExpandButton.clicked += UpdateExpand;
            referenceExpandButton.OnValueChanged.AddListener(UpdateExpand);

            UpdateVRefChange(isVRef.boolValue);

            Debug.Assert(valueType != null);
            Debug.Assert(interfaceType != null);
        }

        private void UpdateVRefChange(bool boolValue)
        {
            if (boolValue)
            {
                _objectContainer.style.display = DisplayStyle.None;
                _referenceContainer.style.display = DisplayStyle.Flex;
                SaintsInterfaceDrawer.SyncInterfaceModeSideEffects(_valueProp, _vRef, true);
            }
            else
            {
                _objectContainer.style.display = DisplayStyle.Flex;
                _referenceContainer.style.display = DisplayStyle.None;
                SaintsInterfaceDrawer.SyncInterfaceModeSideEffects(_valueProp, _vRef, false);
            }
        }

        private void UpdateExpand(bool isExpanded)
        {
            _vRef.serializedObject.ApplyModifiedProperties();
            _saintsRowElement.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
#endif
