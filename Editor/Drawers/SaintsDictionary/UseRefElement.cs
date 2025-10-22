// #if UNITY_2021_3_OR_NEWER
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer;
// using SaintsField.Editor.Drawers.SaintsRowDrawer;
// using SaintsField.Editor.Playa;
// using SaintsField.Editor.UIToolkitElements;
// using SaintsField.Editor.Utils;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;
// using Object = UnityEngine.Object;
//
// namespace SaintsField.Editor.Drawers.SaintsDictionary
// {
//     public class UseRefElement: VisualElement
//     {
//         private readonly SerializedProperty _valueProp;
//         private readonly SerializedProperty _vRef;
//         private readonly ExpandButtonElement _referenceExpandButton;
//         private readonly VisualElement _saintsRowElement;
//         private readonly GeneralUObjectPicker _objectContainer;
//         private readonly VisualElement _referenceContainer;
//
//
//         public UseRefElement(Type interfaceType, SerializedProperty property, SerializedProperty valueProp, IReadOnlyList<Attribute> allAttributes, FieldInfo info, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parentObj)
//         {
//             _valueProp = valueProp;
//             SerializedProperty isVRef = property.FindPropertyRelative("IsVRef") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "IsVRef");
//             _vRef = property.FindPropertyRelative("VRef") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "VRef");
//
//             style.flexDirection = FlexDirection.Row;
//
//             IsVRefButton isVRefButton = new IsVRefButton
//             {
//                 bindingPath = isVRef.propertyPath,
//             };
//             Add(isVRefButton);
//             isVRefButton.RegisterValueChangedCallback(v =>
//             {
//                 UpdateVRefChange(v.newValue);
//             });
//
//             VisualElement columnContainer = new VisualElement
//             {
//                 style =
//                 {
//                     flexGrow = 1,
//                     flexShrink = 1,
//                 },
//             };
//             Add(columnContainer);
//
//             _objectContainer =
//                 new GeneralUObjectPicker(valueType, interfaceType, property.serializedObject.targetObject)
//                     {
//                         bindingPath = valueProp.propertyPath,
//                     };
//             columnContainer.Add(_objectContainer);
//
//             _referenceContainer = new VisualElement();
//             columnContainer.Add(_referenceContainer);
//             VisualElement referenceHContainer = new VisualElement
//             {
//                 style =
//                 {
//                     flexDirection = FlexDirection.Row,
//                 },
//             };
//             _referenceContainer.Add(referenceHContainer);
//
//             // _dropdownIcon = Util.LoadResource<Texture2D>("classic-dropdown.png");
//             // _dropdownRightIcon = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
//             bool expand = allAttributes.Any(each => each is DefaultExpandAttribute)
//                           || _vRef.isExpanded;
//
//             _referenceExpandButton = new ExpandButtonElement
//             {
//                 value = expand,
//             };
//             _referenceExpandButton.SetViewDataKey(_vRef.propertyPath);
//             referenceHContainer.Add(_referenceExpandButton);
//
//             UIToolkitUtils.DropdownButtonField dropdownBtn = UIToolkitUtils.ReferenceDropdownButtonField("", _vRef, this, () => GetTypesImplementingInterface(interfaceType));
//             referenceHContainer.Add(dropdownBtn);
//             dropdownBtn.style.marginLeft = 0;
//             dropdownBtn.ButtonElement.style.borderTopLeftRadius = 0;
//             dropdownBtn.ButtonElement.style.borderBottomLeftRadius = 0;
//             dropdownBtn.labelElement.style.marginLeft = 0;
//             dropdownBtn.TrackPropertyValue(_vRef, vr => dropdownBtn.ButtonLabelElement.text = UIToolkitUtils.GetReferencePropertyLabel(vr));
//
//             // var g = SerializedUtils.GetFileOrProp(parentObj, "VRef");
//             Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
//                 ? ReflectUtils.GetElementType(info.FieldType)
//                 : info.FieldType;
//             MemberInfo vPropInfo = fieldType.GetField("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
//             if (vPropInfo == null)
//             {
//                 vPropInfo = fieldType.GetProperty("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
//             }
//
//             Debug.Assert(vPropInfo != null, fieldType);
//
//             _saintsRowElement = SaintsRowAttributeDrawer.CreateElement(_vRef, "", vPropInfo,
//                 true, new SaintsRowAttribute(inline: true), makeRenderer, doTweenPlayRecorder, parentObj);
//             _referenceContainer.Add(_saintsRowElement);
//
//             UpdateExpand(_referenceExpandButton.value);
//             // _referenceExpandButton.clicked += UpdateExpand;
//             _referenceExpandButton.OnValueChanged.AddListener(UpdateExpand);
//
//             UpdateVRefChange(isVRef.boolValue);
//
//             Debug.Assert(valueType != null);
//             Debug.Assert(interfaceType != null);
//         }
//
//         private void UpdateVRefChange(bool boolValue)
//         {
//             if (boolValue)
//             {
//                 _objectContainer.style.display = DisplayStyle.None;
//                 _referenceContainer.style.display = DisplayStyle.Flex;
//                 if (_valueProp.objectReferenceValue != null)  // Don't keep a reference
//                 {
//                     _valueProp.objectReferenceValue = null;
//                     _valueProp.serializedObject.ApplyModifiedProperties();
//                 }
//             }
//             else
//             {
//                 _objectContainer.style.display = DisplayStyle.Flex;
//                 _referenceContainer.style.display = DisplayStyle.None;
//                 if (_vRef.managedReferenceValue != null)
//                 {
//                     _vRef.managedReferenceValue = null;
//                     _vRef.serializedObject.ApplyModifiedProperties();
//                 }
//             }
//         }
//
//         private IReadOnlyList<Type> _cachedTypesImplementingInterface;
//
//         private IReadOnlyList<Type> GetTypesImplementingInterface(Type interfaceType)
//         {
//             return _cachedTypesImplementingInterface ??= AppDomain.CurrentDomain.GetAssemblies()
//                 .SelectMany(assembly => assembly.GetTypes())
//                 .Where(type => !type.IsAbstract
//                                &&!typeof(Object).IsAssignableFrom(type)
//                                && interfaceType.IsAssignableFrom(type))
//                 .ToArray();
//         }
//
//         private void UpdateExpand(bool isExpanded)
//         {
//             _vRef.serializedObject.ApplyModifiedProperties();
//             _saintsRowElement.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
//         }
//     }
// }
// #endif
