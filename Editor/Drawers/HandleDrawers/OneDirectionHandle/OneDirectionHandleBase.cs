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

namespace SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle
{
    public abstract partial class OneDirectionHandleBase: SaintsPropertyDrawer
    {
        protected struct OneDirectionConstInfo
        {
            public OneDirectionBaseAttribute OneDirectionAttribute;
            public SerializedProperty Property;
            public FieldInfo Info;
            public object Parent;
        }

        protected class OneDirectionInfo
        {
            public OneDirectionConstInfo OneDirectionConstInfo;
            public string Error;
            public Util.TargetWorldPosInfo StartTargetWorldPosInfo;
            public Util.TargetWorldPosInfo EndTargetWorldPosInfo;
        }

        private static OneDirectionInfo GetArrowInfo(OneDirectionConstInfo oneDirectionConstInfo)
        {
            bool isArrayConnecting = oneDirectionConstInfo.OneDirectionAttribute.Start == null
                                     && oneDirectionConstInfo.OneDirectionAttribute.End == null
                                     && oneDirectionConstInfo.OneDirectionAttribute.StartIndex == oneDirectionConstInfo.OneDirectionAttribute.EndIndex;

            if (isArrayConnecting)
            {
                (SerializedProperty arrayProperty, int index, string error) = Util.GetArrayProperty(oneDirectionConstInfo.Property, oneDirectionConstInfo.Info, oneDirectionConstInfo.Parent);
                if (error != "")
                {
                    return new OneDirectionInfo
                    {
                        OneDirectionConstInfo = oneDirectionConstInfo,
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
                Util.TargetWorldPosInfo arrayStartTargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(oneDirectionConstInfo.OneDirectionAttribute.StartSpace, preProperty, oneDirectionConstInfo.Info, oneDirectionConstInfo.Parent);
                if(arrayStartTargetWorldPosInfo.Error != "")
                {
                    Debug.LogError(arrayStartTargetWorldPosInfo.Error);
                    return new OneDirectionInfo
                    {
                        OneDirectionConstInfo = oneDirectionConstInfo,
                        Error = arrayStartTargetWorldPosInfo.Error,
                    };
                }
                Util.TargetWorldPosInfo arrayEndTargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(oneDirectionConstInfo.OneDirectionAttribute.StartSpace, oneDirectionConstInfo.Property, oneDirectionConstInfo.Info, oneDirectionConstInfo.Parent);
                if (arrayEndTargetWorldPosInfo.Error != "")
                {
                    return new OneDirectionInfo
                    {
                        OneDirectionConstInfo = oneDirectionConstInfo,
                        Error = arrayEndTargetWorldPosInfo.Error,
                    };
                }

                // Debug.Log($"{arrayStartTargetWorldPosInfo} -> {arrayEndTargetWorldPosInfo}");

                return new OneDirectionInfo
                {
                    Error = "",
                    OneDirectionConstInfo = oneDirectionConstInfo,
                    StartTargetWorldPosInfo = arrayStartTargetWorldPosInfo,
                    EndTargetWorldPosInfo = arrayEndTargetWorldPosInfo,
                };
            }

            // normal connection
            Util.TargetWorldPosInfo startTargetWorldPosInfo = GetTargetWorldPosInfo(oneDirectionConstInfo.OneDirectionAttribute.Start, oneDirectionConstInfo.OneDirectionAttribute.StartIndex, oneDirectionConstInfo.OneDirectionAttribute.StartSpace, oneDirectionConstInfo.Property, oneDirectionConstInfo.Info, oneDirectionConstInfo.Parent);
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_CONNECT
//             Debug.Log($"normal connect {arrowConstInfo.Property.propertyPath} start {startTargetWorldPosInfo}");
// #endif
            if (startTargetWorldPosInfo.Error != "")
            {
                return new OneDirectionInfo
                {
                    OneDirectionConstInfo = oneDirectionConstInfo,
                    Error = startTargetWorldPosInfo.Error,
                };
            }
            Util.TargetWorldPosInfo endTargetWorldPosInfo = GetTargetWorldPosInfo(oneDirectionConstInfo.OneDirectionAttribute.End, oneDirectionConstInfo.OneDirectionAttribute.EndIndex, oneDirectionConstInfo.OneDirectionAttribute.EndSpace, oneDirectionConstInfo.Property, oneDirectionConstInfo.Info, oneDirectionConstInfo.Parent);
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_CONNECT
//             Debug.Log($"normal connect {arrowConstInfo.Property.propertyPath} end {endTargetWorldPosInfo}");
// #endif
            if (endTargetWorldPosInfo.Error != "")
            {
                return new OneDirectionInfo
                {
                    OneDirectionConstInfo = oneDirectionConstInfo,
                    Error = endTargetWorldPosInfo.Error,
                };
            }

            return new OneDirectionInfo
            {
                Error = "",
                OneDirectionConstInfo = oneDirectionConstInfo,
                StartTargetWorldPosInfo = startTargetWorldPosInfo,
                EndTargetWorldPosInfo = endTargetWorldPosInfo,
            };
        }

        private static Util.TargetWorldPosInfo GetTargetWorldPosInfo(string target, int targetIndex, Space space, SerializedProperty property, FieldInfo info, object parent)
        {
            // use on the field itself
            if (string.IsNullOrEmpty(target))
            {
                return Util.GetPropertyTargetWorldPosInfo(space, property, info, parent);
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
                    if (arr.Length == 0)
                    {
                        return new Util.TargetWorldPosInfo
                        {
                            Error = $"Array is empty",
                        };
                    }

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
                        collectedObject.Add(each);
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

        protected bool OnSceneGUIInternal(SceneView sceneView, OneDirectionInfo oneDirectionInfo)
        {
            if (oneDirectionInfo.Error != "")
            {
                return false;
            }

            (bool okStart, Vector3 worldPosStart) = GetWorldPosFromInfo(oneDirectionInfo.StartTargetWorldPosInfo);
            if (!okStart)
            {
                // Debug.LogError("Start target disposed");
                return false;
            }

            (bool okEnd, Vector3 worldPosEnd) = GetWorldPosFromInfo(oneDirectionInfo.EndTargetWorldPosInfo);
            if (!okEnd)
            {
                // Debug.LogError("End target disposed");
                return false;
            }

            float sqrMagnitude = (worldPosStart - worldPosEnd).sqrMagnitude;

            if(sqrMagnitude < Mathf.Epsilon)
            {
                return false;
            }

            OnSceneDraw(sceneView, oneDirectionInfo, worldPosStart, worldPosEnd);
            // float headLength = arrowInfo.ArrowConstInfo.SaintsArrowAttribute.HeadLength;
            // if(headLength * 2f * headLength * 2f > sqrMagnitude)
            // {
            //     headLength = Mathf.Sqrt(sqrMagnitude) * 0.5f;
            // }
            //
            // (Vector3 tail, Vector3 head, Vector3 arrowheadLeft, Vector3 arrowheadRight) = SaintsDraw.Arrow.GetPoints(
            //     worldPosStart,
            //     worldPosEnd,
            //     arrowHeadLength: headLength,
            //     arrowHeadAngle: arrowInfo.ArrowConstInfo.SaintsArrowAttribute.HeadAngle);
            //
            // using (new HandleColorScoop(arrowInfo.ArrowConstInfo.SaintsArrowAttribute.EColor.GetColor() * new Color(1, 1, 1, arrowInfo.ArrowConstInfo.SaintsArrowAttribute.ColorAlpha)))
            // {
            //     Handles.DrawLine(head, tail);
            //     Handles.DrawLine(head, arrowheadLeft);
            //     Handles.DrawLine(head, arrowheadRight);
            // }

            return true;
        }

        protected abstract void OnSceneDraw(SceneView sceneView, OneDirectionInfo oneDirectionInfo, Vector3 worldPosStart, Vector3 worldPosEnd);

        private static (bool ok, Vector3 worldPos) GetWorldPosFromInfo(Util.TargetWorldPosInfo worldPosInfo)
        {
            if (!worldPosInfo.IsTransform)
            {
                return (true, worldPosInfo.WorldPos);
            }

            Transform trans = worldPosInfo.Transform;
            return trans == null ? (false, Vector3.zero) : (true, trans.position);
        }

#if UNITY_2021_3_OR_NEWER
        ~OneDirectionHandleBase()
        {
            SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
        }
#endif
    }
}
