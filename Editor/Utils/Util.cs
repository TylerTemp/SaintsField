using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Utils
{
    public static class Util
    {
        public static (string error, float value) GetCallbackFloat(object target, string by)
        {
            // SerializedProperty foundProperty = property.FindPropertyRelative(by) ??
            //                                    SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, by);
            // if (foundProperty != null)
            // {
            //     if (foundProperty.propertyType == SerializedPropertyType.Integer)
            //     {
            //         return (foundProperty.intValue, null);
            //     }
            //     if (foundProperty.propertyType == SerializedPropertyType.Float)
            //     {
            //         return (foundProperty.floatValue, null);
            //     }
            //
            //     return (-1, $"Expect int or float for `{by}`, get {foundProperty.propertyType}");
            // }
            //
            // object target = property.serializedObject.targetObject;

            // (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), by);
            // switch (found)
            // {
            //     case (ReflectUtils.GetPropType.NotFound, _):
            //     {
            //         return (-1, $"No field or method named `{by}` found on `{target}`");
            //     }
            //     case (ReflectUtils.GetPropType.Property, PropertyInfo propertyInfo):
            //     {
            //         return ObjToFloat(propertyInfo.GetValue(target));
            //     }
            //     case (ReflectUtils.GetPropType.Field, FieldInfo foundFieldInfo):
            //     {
            //         return ObjToFloat(foundFieldInfo.GetValue(target));
            //     }
            //     case (ReflectUtils.GetPropType.Method, MethodInfo methodInfo):
            //     {
            //         ParameterInfo[] methodParams = methodInfo.GetParameters();
            //         Debug.Assert(methodParams.All(p => p.IsOptional));
            //         // Debug.Assert(methodInfo.ReturnType == typeof(bool));
            //         return ObjToFloat(methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()));
            //     }
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(found), found, null);
            // }

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), by);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (found.Item1 == ReflectUtils.GetPropType.NotFound)
            {
                return ($"No field or method named `{by}` found on `{target}`", -1f);
            }

            if (found.Item1 == ReflectUtils.GetPropType.Property)
            {
                return ObjToFloat(((PropertyInfo)found.Item2).GetValue(target));
            }
            if (found.Item1 == ReflectUtils.GetPropType.Field)
            {
                return ObjToFloat(((FieldInfo)found.Item2).GetValue(target));
            }
            // ReSharper disable once InvertIf
            if (found.Item1 == ReflectUtils.GetPropType.Method)
            {
                MethodInfo methodInfo = (MethodInfo)found.Item2;
                ParameterInfo[] methodParams = methodInfo.GetParameters();
                Debug.Assert(methodParams.All(p => p.IsOptional));
                // Debug.Assert(methodInfo.ReturnType == typeof(bool));
                return ObjToFloat(methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()));
            }
            throw new ArgumentOutOfRangeException(nameof(found), found, null);
        }

        private static (string, float) ObjToFloat(object result)
        {
            switch (result)
            {
                case int intValue:
                    return ("", intValue);
                case float floatValue:
                    return ("", floatValue);
                default:
                    return ($"{result} is neither int or float", -1f);
            }
        }

        public static float BoundFloatStep(float curValue, float start, float end, float step)
        {
            float distance = curValue - start;
            int stepRound = Mathf.RoundToInt(distance / step);
            float newValue = start + stepRound * step;
            return Mathf.Min(newValue, end);
        }

        public static int BoundIntStep(float curValue, float start, float end, int step)
        {
            int useStart = Mathf.CeilToInt(start);
            int useEnd = Mathf.FloorToInt(end);

            float distance = curValue - useStart;
            int stepRound = Mathf.RoundToInt(distance / step);
            int newValue = useStart + stepRound * step;
            return Mathf.Clamp(newValue, useStart, useEnd);
        }

        public static void SetValue(SerializedProperty property, object curItem, object parentObj, Type parentType, FieldInfo field)
        {
            Undo.RecordObject(property.serializedObject.targetObject, "Dropdown");
            // object newValue = curItem;
            // Debug.Log($"set value {parentObj}->{field.Name} = {curItem}");
            if(!parentType.IsValueType)  // reference type
            {
                // Debug.Log($"not struct");
                field.SetValue(parentObj, curItem);
            }
            else  // hack struct :(
            {
                // Debug.Log($"SetValue {property.propertyType}: {curItem}");
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Generic:
                        property.objectReferenceValue = (UnityEngine.Object) curItem;
                        break;
                    case SerializedPropertyType.LayerMask:
                    case SerializedPropertyType.Integer:
                    case SerializedPropertyType.Enum:
                        property.intValue = (int) curItem;
                        Debug.Log($"{property.propertyType}: set ={property.intValue}");
                        break;
                    case SerializedPropertyType.Boolean:
                        property.boolValue = (bool) curItem;
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = (float) curItem;
                        break;
                    case SerializedPropertyType.String:
                        property.stringValue = curItem.ToString();
                        break;
                    case SerializedPropertyType.Color:
                        property.colorValue = (Color) curItem;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        property.objectReferenceValue = (UnityEngine.Object) curItem;
                        break;
                    case SerializedPropertyType.Vector2:
                        property.vector2Value = (Vector2) curItem;
                        break;
                    case SerializedPropertyType.Vector3:
                        property.vector3Value = (Vector3) curItem;
                        break;
                    case SerializedPropertyType.Vector4:
                        property.vector4Value = (Vector4) curItem;
                        break;
                    case SerializedPropertyType.Rect:
                        property.rectValue = (Rect) curItem;
                        break;
                    case SerializedPropertyType.ArraySize:
                        property.arraySize = (int) curItem;
                        break;
                    case SerializedPropertyType.Character:
                        property.intValue = (char) curItem;
                        break;
                    case SerializedPropertyType.AnimationCurve:
                        property.animationCurveValue = (AnimationCurve) curItem;
                        break;
                    case SerializedPropertyType.Bounds:
                        property.boundsValue = (Bounds) curItem;
                        break;
                    // case SerializedPropertyType.Gradient:
                    //     property.gradientValue = (Gradient) curItem;
                    //     break;
                    case SerializedPropertyType.Quaternion:
                        property.quaternionValue = (Quaternion) curItem;
                        break;
                    case SerializedPropertyType.ExposedReference:
                        property.exposedReferenceValue = (UnityEngine.Object) curItem;
                        break;
                    // case SerializedPropertyType.FixedBufferSize:
                    //     property.fixedBufferSize = (int) curItem;
                    //     break;
                    case SerializedPropertyType.Vector2Int:
                        property.vector2IntValue = (Vector2Int) curItem;
                        break;
                    case SerializedPropertyType.Vector3Int:
                        property.vector3IntValue = (Vector3Int) curItem;
                        break;
                    case SerializedPropertyType.RectInt:
                        property.rectIntValue = (RectInt) curItem;
                        break;
                    case SerializedPropertyType.BoundsInt:
                        property.boundsIntValue = (BoundsInt) curItem;
                        break;
#if UNITY_2019_3_OR_NEWER
                    case SerializedPropertyType.ManagedReference:
                        property.managedReferenceValue = (UnityEngine.Object) curItem;
                        break;
#endif
                    case SerializedPropertyType.Gradient:
                    case SerializedPropertyType.FixedBufferSize:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
                }

                property.serializedObject.ApplyModifiedProperties();
            }
        }

        // public static string GetLabelString(SaintsPropertyDrawer.LabelState labelState)
        // {
        //     // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        //     switch (labelState)
        //     {
        //         case SaintsPropertyDrawer.LabelState.None:
        //             return "";
        //
        //         case SaintsPropertyDrawer.LabelState.AsIs:
        //             return null;
        //
        //         case SaintsPropertyDrawer.LabelState.EmptySpace:
        //             return " ";
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(labelState), labelState, null);
        //     }
        // }

        public static Label PrefixLabelUIToolKit(string label, int indentLevel)
        {
            return new Label(label)
            {
                style =
                {
                    flexShrink = 0,
                    flexGrow = 0,
                    minWidth = SaintsPropertyDrawer.LabelBaseWidth - indentLevel * 15,
                    left = SaintsPropertyDrawer.LabelLeftSpace,
                    unityTextAlign = TextAnchor.MiddleLeft,
                },
            };
        }

        public static int ListIndexOfAction<T>(IReadOnlyList<T> lis, Func<T, bool> callback)
        {
            foreach (int index in Enumerable.Range(0, lis.Count))
            {
                if (callback(lis[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        public static void FixLabelWidthLoopUIToolkit(Label label)
        {
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
            FixLabelWidthUIToolkit(label);
            label.RegisterCallback<GeometryChangedEvent>(evt => FixLabelWidthUIToolkit((Label)evt.target));
#endif
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void FixLabelWidthUIToolkit(Label label)
        {
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
            if(label.style.width != StyleKeyword.Auto)
            {
                label.style.width = StyleKeyword.Auto;
            }
#endif
        }
    }
}
