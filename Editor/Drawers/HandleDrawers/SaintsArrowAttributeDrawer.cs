#if SAINTSFIELD_SAINTSDRAW && !SAINTSFIELD_SAINTSDRAW_DISABLE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
            bool isArrayConnecting = arrowConstInfo.SaintsArrowAttribute.Start == null
                                     && arrowConstInfo.SaintsArrowAttribute.End == null
                                     && arrowConstInfo.SaintsArrowAttribute.StartIndex == arrowConstInfo.SaintsArrowAttribute.EndIndex;

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
                Util.TargetWorldPosInfo arrayStartTargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(arrowConstInfo.SaintsArrowAttribute.Space, preProperty, arrowConstInfo.Info, arrowConstInfo.Parent);
                if(arrayStartTargetWorldPosInfo.Error != "")
                {
                    Debug.LogError(arrayStartTargetWorldPosInfo.Error);
                    return new ArrowInfo
                    {
                        ArrowConstInfo = arrowConstInfo,
                        Error = arrayStartTargetWorldPosInfo.Error,
                    };
                }
                Util.TargetWorldPosInfo arrayEndTargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(arrowConstInfo.SaintsArrowAttribute.Space, arrowConstInfo.Property, arrowConstInfo.Info, arrowConstInfo.Parent);
                if (arrayEndTargetWorldPosInfo.Error != "")
                {
                    return new ArrowInfo
                    {
                        ArrowConstInfo = arrowConstInfo,
                        Error = arrayEndTargetWorldPosInfo.Error,
                    };
                }

                return new ArrowInfo
                {
                    Error = "",
                    ArrowConstInfo = arrowConstInfo,
                    StartTargetWorldPosInfo = arrayStartTargetWorldPosInfo,
                    EndTargetWorldPosInfo = arrayEndTargetWorldPosInfo,
                };
            }

            // normal connection
            Util.TargetWorldPosInfo startTargetWorldPosInfo = GetTargetWorldPosInfo(arrowConstInfo.SaintsArrowAttribute.Start, arrowConstInfo.SaintsArrowAttribute.StartIndex, arrowConstInfo.SaintsArrowAttribute.Space, arrowConstInfo.Property, arrowConstInfo.Info, arrowConstInfo.Parent);
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_CONNECT
//             Debug.Log($"normal connect {arrowConstInfo.Property.propertyPath} start {startTargetWorldPosInfo}");
// #endif
            if (startTargetWorldPosInfo.Error != "")
            {
                return new ArrowInfo
                {
                    ArrowConstInfo = arrowConstInfo,
                    Error = startTargetWorldPosInfo.Error,
                };
            }
            Util.TargetWorldPosInfo endTargetWorldPosInfo = GetTargetWorldPosInfo(arrowConstInfo.SaintsArrowAttribute.End, arrowConstInfo.SaintsArrowAttribute.EndIndex, arrowConstInfo.SaintsArrowAttribute.Space, arrowConstInfo.Property, arrowConstInfo.Info, arrowConstInfo.Parent);
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_CONNECT
//             Debug.Log($"normal connect {arrowConstInfo.Property.propertyPath} end {endTargetWorldPosInfo}");
// #endif
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

        private Util.TargetWorldPosInfo GetTargetWorldPosInfo(string target, int targetIndex, Space space, SerializedProperty property, FieldInfo info, object parent)
        {
            // use on the field itself
            if (string.IsNullOrEmpty(target))
            {
                return Util.GetPropertyTargetWorldPosInfo(space, property, info, parent);
                // int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                // if (propertyIndex == -1)  // not in array
                // {
                //     return Util.GetPropertyTargetWorldPosInfo(space, property, info, parent);
                // }
                // // find element in array
                // (SerializedProperty arrayProperty, int _, string arrayError) = Util.GetArrayProperty(property, info, parent);
                // if (arrayError != "")
                // {
                //     return new Util.TargetWorldPosInfo
                //     {
                //         Error = arrayError,
                //     };
                // }

                // SerializedProperty targetProperty = arrayProperty.GetArrayElementAtIndex(targetIndex);
                // return Util.GetPropertyTargetWorldPosInfo(space, targetProperty, info, parent);
            }

            // it's either another field, or it's a callback
            // another field will be tricky to get (if it's inside a normal struct/class), so we just treat it as a callback
            // this decorator will only draw one arrow, so if the target is an array/list, use the index to fetch only one element

            (string error, object result) = Util.GetOf<object>(target, null, property, info, parent);
            if (error != "")
            {
                return new Util.TargetWorldPosInfo
                {
                    Error = error,
                };
            }

            switch (result)
            {
                case null:
                    return new Util.TargetWorldPosInfo
                    {
                        Error = $"{target} is null",
                    };
                case GameObject go:
                    return new Util.TargetWorldPosInfo
                    {
                        Error = "",
                        IsTransform = true,
                        Transform = go.transform,
                    };
                case Component comp:
                    return new Util.TargetWorldPosInfo
                    {
                        Error = "",
                        IsTransform = true,
                        Transform = comp.transform,
                    };
                case Vector2 v2:
                    return Util.GetValueFromVector(space, property, v2);
                case Vector3 v3:
                    return Util.GetValueFromVector(space, property, v3);
                case Array arr:
                {
                    int useIndex = targetIndex < 0 ? arr.Length + targetIndex : targetIndex;

                    if (arr.Length <= useIndex)
                    {
                        return new Util.TargetWorldPosInfo
                        {
                            Error = $"Array {target} length {arr.Length} is less than {useIndex}",
                        };
                    }

                    object targetObj = arr.GetValue(useIndex);
                    switch (targetObj)
                    {
                        case null:
                            return new Util.TargetWorldPosInfo
                            {
                                Error = $"Array {target} index {targetIndex} is null",
                            };
                        case GameObject goArr:
                            return new Util.TargetWorldPosInfo
                            {
                                Error = "", IsTransform = true, Transform = goArr.transform,
                            };
                        case Component compArr:
                            return new Util.TargetWorldPosInfo
                            {
                                Error = "", IsTransform = true, Transform = compArr.transform,
                            };
                        case Vector2 v2:
                            return Util.GetValueFromVector(space, property, v2);
                        case Vector3 v3:
                            return Util.GetValueFromVector(space, property, v3);
                        default:
                            return new Util.TargetWorldPosInfo
                            {
                                Error = $"Array {target} index {targetIndex} is not GameObject or Component",
                            };
                    }
                }
                case IEnumerable ie:
                {
                    object targetObj = null;
                    bool found = false;
                    List<object> collectedObject = new List<object>();
                    foreach ((object each, int index) in ie.Cast<object>().WithIndex())
                    {
                        if (index == targetIndex)
                        {
                            targetObj = each;
                            found = true;
                            break;
                        }
                    }

                    if (!found && targetIndex < 0)
                    {
                        int useIndex = collectedObject.Count + targetIndex;
                        if (useIndex >= 0 && useIndex < collectedObject.Count)
                        {
                            targetObj = collectedObject[useIndex];
                            found = true;
                        }
                        else
                        {
                            return new Util.TargetWorldPosInfo
                            {
                                Error =
                                    $"IEnumerable {target} index {targetIndex} out of range ${collectedObject.Count} in {ie}",
                            };
                        }
                    }

                    if (targetObj == null)
                    {
                        return new Util.TargetWorldPosInfo
                        {
                            Error = found? $"IEnumerable {target} index {targetIndex} is null": $"IEnumerable {target} index {targetIndex} not found",
                        };
                    }

                    switch (targetObj)
                    {
                        case GameObject goIE:
                            return new Util.TargetWorldPosInfo
                            {
                                Error = "", IsTransform = true, Transform = goIE.transform,
                            };
                        case Component compIE:
                            return new Util.TargetWorldPosInfo
                            {
                                Error = "", IsTransform = true, Transform = compIE.transform,
                            };
                        case Vector2 v2:
                            return Util.GetValueFromVector(space, property, v2);
                        case Vector3 v3:
                            return Util.GetValueFromVector(space, property, v3);
                        default:
                            return new Util.TargetWorldPosInfo
                            {
                                Error = $"IEnumerable {target} index {targetIndex} is not GameObject or Component",
                            };
                    }
                }
                default:
                    return new Util.TargetWorldPosInfo
                    {
                        Error = $"{target} type {result.GetType()} is not supported",
                    };
            }
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

            float sqrMagnitude = (worldPosStart - worldPosEnd).sqrMagnitude;

            if(sqrMagnitude < Mathf.Epsilon)
            {
                return;
            }

            float headLength = arrowInfo.ArrowConstInfo.SaintsArrowAttribute.HeadLength;
            if(headLength * 2f * headLength * 2f > sqrMagnitude)
            {
                headLength = Mathf.Sqrt(sqrMagnitude) * 0.5f;
            }

            (Vector3 tail, Vector3 head, Vector3 arrowheadLeft, Vector3 arrowheadRight) = SaintsDraw.Arrow.GetPoints(
                worldPosStart,
                worldPosEnd,
                arrowHeadLength: headLength,
                arrowHeadAngle: arrowInfo.ArrowConstInfo.SaintsArrowAttribute.HeadAngle);

            using (new HandleColorScoop(arrowInfo.ArrowConstInfo.SaintsArrowAttribute.EColor.GetColor()))
            {
                Handles.DrawLine(head, tail);
                Handles.DrawLine(head, arrowheadLeft);
                Handles.DrawLine(head, arrowheadRight);
                // Handles.DrawLine(worldPosStart, worldPosEnd);
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
            Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(saintsArrowAttribute.Space, property, info, parent);
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

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            // ReSharper disable once MergeIntoNegatedPattern
            if (_arrowInfoUIToolkit == null || _arrowInfoUIToolkit.Error != "")
            {
                return;
            }

            if (!_arrowInfoUIToolkit.StartTargetWorldPosInfo.IsTransform || !_arrowInfoUIToolkit.EndTargetWorldPosInfo.IsTransform)
            {
                _arrowInfoUIToolkit = GetArrowInfo(_arrowInfoUIToolkit.ArrowConstInfo);
            }
        }

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

#else
using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    [CustomPropertyDrawer(typeof(SaintsArrowAttribute))]
    public class SaintsArrowAttributeDrawer : SaintsPropertyDrawer
    {
        private const string Url = "https://github.com/TylerTemp/SaintsDraw";
        private const string ErrorMessage = "Requires SaintsDraw (>= 1.0.4): " + Url;

        #region IMGUI
        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return ImGuiHelpBox.GetHeight(ErrorMessage, width, MessageType.Error) + SingleLineHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            Rect leftRect = ImGuiHelpBox.Draw(position, ErrorMessage, MessageType.Error);
            (Rect buttonRect, Rect emptyRect) = RectUtils.SplitHeightRect(leftRect, SingleLineHeight);
            if (GUI.Button(buttonRect, "Open"))
            {
                Application.OpenURL(Url);
            }
            return emptyRect;
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit


        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                },
            };

            root.Add(new HelpBox
            {
                text = ErrorMessage,
                messageType = HelpBoxMessageType.Error,
            });
            root.Add(new Button(() => Application.OpenURL(Url))
            {
                text = "Open",
            });

            return root;
        }
        #endregion

#endif
    }
}
#endif
