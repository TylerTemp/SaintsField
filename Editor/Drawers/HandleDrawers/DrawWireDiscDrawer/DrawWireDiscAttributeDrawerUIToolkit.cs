// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using SaintsField.Editor.Utils;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace SaintsField.Editor.Drawers.HandleDrawers.DrawWireDiscDrawer
// {
//     public partial class DrawWireDiscAttributeDrawer
//     {
//         private static string NameDrawLabel(SerializedProperty property) => $"{property.propertyPath}_DrawWireDisc";
//
//         private WireDiscInfo _wireDiscInfo;
//
//         protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
//             ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
//         {
//             DrawWireDiscAttribute drawWireDiscAttribute = (DrawWireDiscAttribute)saintsAttribute;
//             Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(drawWireDiscAttribute.Space, property, info, parent);
//             if (targetWorldPosInfo.Error != "")
//             {
//                 return new HelpBox(targetWorldPosInfo.Error, HelpBoxMessageType.Error);
//             }
//
//             _wireDiscInfo = new WireDiscInfo
//             {
//                 Content = drawWireDiscAttribute.Content,
//                 ActualContent = drawWireDiscAttribute.Content,
//                 IsCallback = drawWireDiscAttribute.IsCallback,
//                 EColor = drawWireDiscAttribute.EColor,
//                 TargetWorldPosInfo = targetWorldPosInfo,
//             };
//
//             return null;
//         }
//
//         protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
//             int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
//             Action<object> onValueChangedCallback, FieldInfo info, object parent)
//         {
//             VisualElement child = new VisualElement
//             {
//                 name = NameDrawLabel(property),
//             };
//             child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUIUIToolkit);
//             child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
//             container.Add(child);
//         }
//
//         protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
//             int index,
//             VisualElement container, Action<object> onValueChanged, FieldInfo info)
//         {
//             if (_labelInfoUIToolkit.IsCallback)
//             {
//                 object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
//
//                 (string error, object value) =
//                     Util.GetOf<object>(_labelInfoUIToolkit.Content, null, property, info, parent);
//
//                 if (error != "")
//                 {
// #if SAINTSFIELD_DEBUG
//                     Debug.LogError(error);
// #endif
//                     return;
//                 }
//
//                 if (value is IWrapProp wrapProp)
//                 {
//                     value = Util.GetWrapValue(wrapProp);
//                 }
//
//                 _labelInfoUIToolkit.ActualContent = $"{value}";
//             }
//
//             if (!_labelInfoUIToolkit.TargetWorldPosInfo.IsTransform)
//             {
//                 DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;
//                 object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
//                 if(parent != null)
//                 {
//                     _labelInfoUIToolkit.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
//                 }
//             }
//         }
//
//         // private GUIStyle _guiStyleUIToolkit;
//
//         private void OnSceneGUIUIToolkit(SceneView sceneView)
//         {
//             OnSceneGUIInternal(sceneView, _labelInfoUIToolkit);
//         }
//     }
// }
