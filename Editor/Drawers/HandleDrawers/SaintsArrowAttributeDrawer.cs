using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if SAINTSFIELD_SAINTSDRAW || true

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    [CustomPropertyDrawer(typeof(SaintsArrowAttribute))]
    public class SaintsArrowAttributeDrawer: SaintsPropertyDrawer
    {
        private struct ArrowConstInfo
        {
            public SaintsArrowAttribute SaintsArrowAttribute;
            public SerializedProperty Property;
            public FieldInfo Info;
            public object Parent;
        }

        private class ArrowInfo
        {
            public ArrowConstInfo ArrowConstInfo;
            public string Error;
            public Util.TargetWorldPosInfo StartTargetWorldPosInfo;
            public Util.TargetWorldPosInfo EndTargetWorldPosInfo;
        }

        private ArrowInfo GetArrowInfo(ArrowConstInfo arrowConstInfo)
        {
            bool isArrayConnecting = arrowConstInfo.SaintsArrowAttribute.StartTarget is null
                                     && arrowConstInfo.SaintsArrowAttribute.EndTarget is null;

            if (isArrayConnecting)
            {
                (SerializedProperty arrayProperty, int index, string error) = Util.GetArrayProperty(arrowConstInfo.Property, arrowConstInfo.Info, arrowConstInfo.Parent);
                if (error != "")
                {
                    return new ArrowInfo
                    {
                        ArrowConstInfo = arrowConstInfo,
                        Error = error,
                    };
                }
                int arraySize = arrayProperty.arraySize;
                // 2 -> 1, 1 -> 0; first element do nothing
                if (index == 0)  // first element
                {
                    return null;
                }

                if (arraySize <= 1)
                {
                    return null;
                }

                SerializedProperty preProperty = arrayProperty.GetArrayElementAtIndex(index - 1);
                Util.TargetWorldPosInfo startTargetWorldPosInfo = Util.GetTargetWorldPosInfo(arrowConstInfo.SaintsArrowAttribute.Space, preProperty, arrowConstInfo.Info, arrowConstInfo.Parent);
                if(startTargetWorldPosInfo.Error != "")
                {
                    Debug.LogError(startTargetWorldPosInfo.Error);
                    return new ArrowInfo
                    {
                        ArrowConstInfo = arrowConstInfo,
                        Error = startTargetWorldPosInfo.Error,
                    };
                }
                Util.TargetWorldPosInfo endTargetWorldPosInfo = Util.GetTargetWorldPosInfo(arrowConstInfo.SaintsArrowAttribute.Space, arrowConstInfo.Property, arrowConstInfo.Info, arrowConstInfo.Parent);
                if (endTargetWorldPosInfo.Error != "")
                {
                    return new ArrowInfo
                    {
                        ArrowConstInfo = arrowConstInfo,
                        Error = endTargetWorldPosInfo.Error,
                    };
                }

                return new ArrowInfo
                {
                    Error = "",
                    ArrowConstInfo = arrowConstInfo,
                    StartTargetWorldPosInfo = startTargetWorldPosInfo,
                    EndTargetWorldPosInfo = endTargetWorldPosInfo,
                };
            }

            return null;
        }

        private void OnSceneGUIInternal(SceneView sceneView, ArrowInfo arrowInfo)
        {
            if (arrowInfo.Error != "")
            {
                return;
            }

            (bool okStart, Vector3 worldPosStart) = GetWorldPosFromInfo(arrowInfo.StartTargetWorldPosInfo);
            if (!okStart)
            {
                // Debug.LogError("Start target disposed");
                return;
            }

            (bool okEnd, Vector3 worldPosEnd) = GetWorldPosFromInfo(arrowInfo.EndTargetWorldPosInfo);
            if (!okEnd)
            {
                // Debug.LogError("End target disposed");
                return;
            }

            // var arrowPoints = SaintsDraw.Arrow.GetPoints()

            using (new HandleColorScoop(arrowInfo.ArrowConstInfo.SaintsArrowAttribute.EColor.GetColor()))
            {
                Handles.DrawLine(worldPosStart, worldPosEnd);
            }
        }

        private (bool ok, Vector3 worldPos) GetWorldPosFromInfo(Util.TargetWorldPosInfo worldPosInfo)
        {
            if (!worldPosInfo.IsTransform)
            {
                return (true, worldPosInfo.WorldPos);
            }

            Transform trans = worldPosInfo.Transform;
            return trans == null ? (false, Vector3.zero) : (true, trans.position);
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit
        private static string NameSaintsArrow(SerializedProperty property) => $"{property.propertyPath}_SaintsArrow";

        private ArrowInfo _arrowInfoUIToolkit;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            SaintsArrowAttribute saintsArrowAttribute = (SaintsArrowAttribute)saintsAttribute;
            Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetTargetWorldPosInfo(saintsArrowAttribute.Space, property, info, parent);
            if (targetWorldPosInfo.Error != "")
            {
                return new HelpBox(targetWorldPosInfo.Error, HelpBoxMessageType.Error);
            }
            return null;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            ArrowConstInfo arrowConstInfo = new ArrowConstInfo
            {
                SaintsArrowAttribute = (SaintsArrowAttribute) saintsAttribute,
                Property = property,
                Info = info,
                Parent = parent,
            };

            _arrowInfoUIToolkit = GetArrowInfo(arrowConstInfo);

            VisualElement child = new VisualElement
            {
                name = NameSaintsArrow(property),
            };
            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUIUIToolkit);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

//         protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
//             int index,
//             VisualElement container, Action<object> onValueChanged, FieldInfo info)
//         {
//             if (_arrowInfoUIToolkit.IsCallback)
//             {
//                 object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
//
//                 (string error, object value) =
//                     Util.GetOf<object>(_arrowInfoUIToolkit.Content, null, property, info, parent);
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
//                 _arrowInfoUIToolkit.ActualContent = $"{value}";
//             }
//
//             if (!_arrowInfoUIToolkit.TargetWorldPosInfo.IsTransform)
//             {
//                 DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;
//                 object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
//                 if(parent != null)
//                 {
//                     _arrowInfoUIToolkit.TargetWorldPosInfo = Util.GetTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
//                 }
//             }
//         }

        private GUIStyle _guiStyleUIToolkit;

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            if (_arrowInfoUIToolkit is null)
            {
                // Debug.Log($"no config");
                return;
            }

            // Debug.Log($"render {_arrowInfoUIToolkit}");

            OnSceneGUIInternal(sceneView, _arrowInfoUIToolkit);
        }

        #endregion

#endif
    }
}

#endif
