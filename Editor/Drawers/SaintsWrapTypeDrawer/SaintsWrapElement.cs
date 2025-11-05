using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsWrapTypeDrawer
{
    public class SaintsWrapElement: VisualElement
    {
        private readonly SerializedProperty _unityProp;
        private readonly VisualElement _saintsRowElement;
        private readonly GeneralUObjectPicker _objectContainer;
        private readonly VisualElement _referenceContainer;
        private readonly SerializedProperty _vRef;

        public SaintsWrapElement(bool manuallySave, Type nonUnityType, SerializedProperty property, IReadOnlyList<Attribute> allAttributes, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parentObj)
        {
            _unityProp = property.FindPropertyRelative("V");
            SerializedProperty isVRef = property.FindPropertyRelative("IsVRef") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "IsVRef");
            SerializedProperty vRef = property.FindPropertyRelative("VRef") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "VRef");
            _vRef = vRef;

            style.flexDirection = FlexDirection.Row;

            IsVRefButton isVRefButton = new IsVRefButton
            {
                bindingPath = isVRef.propertyPath,
                value = isVRef.boolValue,
            };
            Add(isVRefButton);
            isVRefButton.RegisterValueChangedCallback(v =>
            {
                // Debug.Log($"isVRefButton={v.newValue}");
                UpdateVRefChange(v.newValue);
                if (manuallySave)
                {
                    isVRef.boolValue = v.newValue;
                    isVRef.serializedObject.ApplyModifiedProperties();
                }
            });

            bool canNotAssignUnityObject =
                nonUnityType.IsClass && !typeof(UnityEngine.Object).IsAssignableFrom(nonUnityType)
                || nonUnityType.IsValueType;

            if (canNotAssignUnityObject)
            {
                isVRefButton.style.display = DisplayStyle.None;
                if (!isVRef.boolValue)
                {
                    isVRef.boolValue = true;
                    isVRef.serializedObject.ApplyModifiedProperties();
                }
            }

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
                new GeneralUObjectPicker(_unityProp.propertyPath, _unityProp.objectReferenceValue, typeof(UnityEngine.Object), nonUnityType, property.serializedObject.targetObject)
                    {
                        // bindingPath = _unityProp.propertyPath,
                        // value = _unityProp.objectReferenceValue,
                    };
            // Debug.Log($"_unityProp.objectReferenceValue={_unityProp.objectReferenceValue}");
            // Auto bind is broken for some reason... manually assign it

            // _objectContainer.TrackPropertyValue(_unityProp, u => Debug.Log(u.objectReferenceValue));
            if(manuallySave)
            {
                _objectContainer.RegisterValueChangedCallback(v =>
                {
                    // Debug.Log($"_objectContainer={v.newValue}");
                    Object newValue = v.newValue;
                    if (!RuntimeUtil.IsNull(newValue))
                    {
                        (bool found, Object result) = RevertComponents(newValue, nonUnityType);
                        if (!found)
                        {
                            Debug.LogWarning($"type {nonUnityType} not found on {newValue}");
                            return;
                        }

                        newValue = result;
                    }
                    // ReSharper disable once InvertIf
                    if(_unityProp.objectReferenceValue != newValue)
                    {
                        _unityProp.objectReferenceValue = newValue;
                        _unityProp.serializedObject.ApplyModifiedProperties();
                    }
                });
            }
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
            bool expand = allAttributes.Any(each => each is DefaultExpandAttribute)
                          || vRef.isExpanded;

            ExpandButtonElement referenceExpandButton = new ExpandButtonElement
            {
                value = expand,
            };
            referenceExpandButton.SetViewDataKey(vRef.propertyPath);
            referenceHContainer.Add(referenceExpandButton);

            UIToolkitUtils.DropdownButtonField dropdownBtn = UIToolkitUtils.ReferenceDropdownButtonField("", vRef, referenceHContainer, () => GetTypesImplementingInterface(nonUnityType));
            referenceHContainer.Add(dropdownBtn);
            dropdownBtn.style.marginLeft = 0;
            dropdownBtn.ButtonElement.style.borderTopLeftRadius = 0;
            dropdownBtn.ButtonElement.style.borderBottomLeftRadius = 0;
            dropdownBtn.labelElement.style.marginLeft = 0;
            dropdownBtn.TrackPropertyValue(vRef, vr => dropdownBtn.ButtonLabelElement.text = UIToolkitUtils.GetReferencePropertyLabel(vr));

            // var g = SerializedUtils.GetFileOrProp(parentObj, "VRef");

            FieldInfo vPropInfo = typeof(SaintsSerializedProperty).GetField("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            // if (vPropInfo == null)
            // {
            //     vPropInfo = wrappedType.GetProperty("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            // }

            Debug.Assert(vPropInfo != null, nonUnityType);

            _saintsRowElement = SaintsRowAttributeDrawer.CreateElement(vRef, "", vPropInfo,
                true, new SaintsRowAttribute(inline: true), makeRenderer, doTweenPlayRecorder, parentObj);
            _referenceContainer.Add(_saintsRowElement);

            UpdateExpand(referenceExpandButton.value);
            // _referenceExpandButton.clicked += UpdateExpand;
            referenceExpandButton.OnValueChanged.AddListener(UpdateExpand);

            UpdateVRefChange(isVRef.boolValue);

            Debug.Assert(nonUnityType != null);
        }

        private static (bool found, Object result) RevertComponents(Object newValue, Type nonUnityType)
        {
            if (newValue is GameObject go)
            {
                foreach (Component component in go.GetComponents<Component>())
                {
                    if (nonUnityType.IsInstanceOfType(component))
                    {
                        return (true, component);
                    }
                }

                return (false, null);
            }

            bool found = nonUnityType.IsInstanceOfType(newValue);
            return found ? (true, newValue) : (false, null);
        }

        private void UpdateVRefChange(bool boolValue)
        {
            if (boolValue)
            {
                _objectContainer.style.display = DisplayStyle.None;
                _referenceContainer.style.display = DisplayStyle.Flex;
                if (_unityProp.objectReferenceValue != null)  // Don't keep a reference
                {
                    _unityProp.objectReferenceValue = null;
                    _unityProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                _objectContainer.style.display = DisplayStyle.Flex;
                _referenceContainer.style.display = DisplayStyle.None;
                if (_vRef.managedReferenceValue != null)
                {
                    _vRef.managedReferenceValue = null;
                    _vRef.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private IReadOnlyList<Type> _cachedTypesImplementingInterface;

        private IReadOnlyList<Type> GetTypesImplementingInterface(Type interfaceType)
        {
            return _cachedTypesImplementingInterface ??= AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract
                               &&!typeof(UnityEngine.Object).IsAssignableFrom(type)
                               && interfaceType.IsAssignableFrom(type))
                .ToArray();
        }

        private void UpdateExpand(bool isExpanded)
        {
            _saintsRowElement.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
