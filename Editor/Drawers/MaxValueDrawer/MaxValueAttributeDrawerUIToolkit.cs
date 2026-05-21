#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework.Internal.Execution;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.MaxValueDrawer
{
    public partial class MaxValueAttributeDrawer
    {

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__MaxValue_HelpBox";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            MaxValueAttribute maxValueAttribute = (MaxValueAttribute)saintsAttribute;

            Refresh();
            helpBox.TrackPropertyValue(property, _ => Refresh());
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(Refresh);
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(Refresh);
                UIToolkitUtils.Unbind(helpBox);
            });
            return;

            void Refresh()
            {
                TrackValue(property, maxValueAttribute, helpBox, onValueChangedCallback, info, parent);
            }
        }

        private static void TrackValue(SerializedProperty property, MaxValueAttribute maxValueAttribute,
            HelpBox helpBox, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            List<string> errors = new List<string>(4);
            // bool changed = false;
            for (int index = 0; index < maxValueAttribute.Positions.Count; index++)
            {
                NumberLimitParam requiredLimit = maxValueAttribute.Positions[index];
                if (requiredLimit.SourceType == SourceType.NoLimit)
                {
                    continue;
                }

                if (requiredLimit.SourceType == SourceType.Callback)
                {
                    (string callbackError, MemberInfo _, object result) =
                        Util.GetOf<object>(requiredLimit.Callback, null, property, info, parent, null);
                    if (callbackError != "")
                    {
                        errors.Add(callbackError);
                        continue;
                    }

                    NumberLimitParam newLimit = new NumberLimitParam(result);
                    if (newLimit.SourceType == SourceType.NoLimit)
                    {
                        continue;
                    }

                    if (newLimit.SourceType == SourceType.Callback)
                    {
                        errors.Add($"Callback on position {index} as `{requiredLimit.Callback}` returns a string value not a number");
                        continue;
                    }

                    requiredLimit = newLimit;
                }

                (string sourceError, NumberLimitParam sourceNumber, Action<object> apply) = ParsePropertySource(index, property);
                if (sourceError != string.Empty)
                {
                    errors.Add(sourceError);
                    continue;
                }

                CheckLimitAndApply(sourceNumber, requiredLimit, apply);
            }

            string error = string.Join("\n", errors);

            UIToolkitUtils.SetHelpBox(helpBox, error);
        }


        private static (string sourceError, NumberLimitParam sourceNumber, Action<object> apply) ParsePropertySource(int index, SerializedProperty property)
        {
            if (index == 0)
            {
                (string propDirectError, NumberLimitParam propDirectNumber, Action<object> apply) r = ParsePropertyDirect(property);
                if (r.propDirectError == string.Empty)
                {
                    return r;
                }
            }

            return ParsePropertyAtPosition(index, property);
        }

        private static (string propDirectError, NumberLimitParam propDirectNumber, Action<object> apply) ParsePropertyDirect(SerializedProperty property)
        {
            switch (property.numericType)
            {
                case SerializedPropertyNumericType.Unknown:
                    return ($"Not supported type {property.propertyType}({property.numericType})", default, default);
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.Int32:  // ..., int, use int
                    return (string.Empty, new NumberLimitParam(property.intValue), obj =>
                    {
                        property.intValue = Convert.ToInt32(obj);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                case SerializedPropertyNumericType.UInt32:  // uint, use long
                    return (string.Empty, new NumberLimitParam(property.uintValue), obj =>
                    {
                        property.uintValue = Convert.ToUInt32(obj);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                case SerializedPropertyNumericType.Int64:  // long, use long
                    return (string.Empty, new NumberLimitParam(property.longValue), obj =>
                    {
                        property.longValue = Convert.ToInt64(obj);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                case SerializedPropertyNumericType.UInt64: // ulong, use ulong
                    return (string.Empty, new NumberLimitParam(property.ulongValue), obj =>
                    {
                        property.ulongValue = Convert.ToUInt64(obj);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                case SerializedPropertyNumericType.Float:  // float, use double
                    return (string.Empty, new NumberLimitParam(property.floatValue), obj =>
                    {
                        property.floatValue = Convert.ToSingle(obj);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                case SerializedPropertyNumericType.Double:  // double, use double
                    return (string.Empty, new NumberLimitParam(property.doubleValue), obj =>
                    {
                        property.doubleValue = Convert.ToDouble(obj);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static (string sourceError, NumberLimitParam sourceNumber, Action<object> apply) ParsePropertyAtPosition(int index, SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.vector2Value.x), (Action<object>)(obj =>
                        {
                            property.vector2Value = new Vector2(Convert.ToSingle(obj), property.vector2Value.y);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        1 => (string.Empty, new NumberLimitParam(property.vector2Value.y), (Action<object>)(obj =>
                        {
                            property.vector2Value = new Vector2(property.vector2Value.x, Convert.ToSingle(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for vector2 type", default, null),
                    };
                case SerializedPropertyType.Vector2Int:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.vector2IntValue.x), (Action<object>)(obj =>
                        {
                            property.vector2IntValue = new Vector2Int(Convert.ToInt32(obj), property.vector2IntValue.y);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        1 => (string.Empty, new NumberLimitParam(property.vector2IntValue.y), (Action<object>)(obj =>
                        {
                            property.vector2IntValue = new Vector2Int(property.vector2IntValue.x, Convert.ToInt32(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for vector2Int type", default, null),
                    };
                case SerializedPropertyType.Vector3:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.vector3Value.x), (Action<object>)(obj =>
                        {
                            property.vector3Value = new Vector3(Convert.ToSingle(obj), property.vector3Value.y, property.vector3Value.z);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        1 => (string.Empty, new NumberLimitParam(property.vector3Value.y), (Action<object>)(obj =>
                        {
                            property.vector3Value = new Vector3(property.vector3Value.x, Convert.ToSingle(obj), property.vector3Value.z);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        2 => (string.Empty, new NumberLimitParam(property.vector3Value.z), (Action<object>)(obj =>
                        {
                            property.vector3Value = new Vector3(property.vector3Value.x, property.vector3Value.y, Convert.ToSingle(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for vector3 type", default, null),
                    };
                case SerializedPropertyType.Vector3Int:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.vector3IntValue.x), (Action<object>)(obj =>
                        {
                            property.vector3IntValue = new Vector3Int(Convert.ToInt32(obj), property.vector3IntValue.y, property.vector3IntValue.z);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        1 => (string.Empty, new NumberLimitParam(property.vector3IntValue.y), (Action<object>)(obj =>
                        {
                            property.vector3IntValue = new  Vector3Int(property.vector3IntValue.x, Convert.ToInt32(obj), property.vector3IntValue.z);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        2 => (string.Empty, new NumberLimitParam(property.vector3IntValue.z), (Action<object>)(obj =>
                        {
                            property.vector3IntValue = new Vector3Int(property.vector3IntValue.x, property.vector3IntValue.y, Convert.ToInt32(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for vector3Int type", default, null),
                    };
                case SerializedPropertyType.Vector4:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.vector4Value.x), (Action<object>)(obj =>
                        {
                            property.vector4Value = new Vector4(Convert.ToSingle(obj), property.vector4Value.y, property.vector4Value.z, property.vector4Value.w);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        1 => (string.Empty, new NumberLimitParam(property.vector4Value.y), (Action<object>)(obj =>
                        {
                            property.vector4Value = new Vector4(property.vector4Value.x, Convert.ToSingle(obj), property.vector4Value.z, property.vector4Value.w);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        2 => (string.Empty, new NumberLimitParam(property.vector4Value.z), (Action<object>)(obj =>
                        {
                            property.vector4Value = new Vector4(property.vector4Value.x, property.vector4Value.y, Convert.ToSingle(obj), property.vector4Value.w);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        3 => (string.Empty, new NumberLimitParam(property.vector4Value.w), (Action<object>)(obj =>
                        {
                            property.vector4Value = new Vector4(property.vector4Value.x, property.vector4Value.y, property.vector4Value.z, Convert.ToSingle(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for vector4 type", default, null),
                    };
                case SerializedPropertyType.Quaternion:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.quaternionValue.x), (Action<object>)(obj =>
                        {
                            Quaternion q = property.quaternionValue;
                            property.quaternionValue = new Quaternion(Convert.ToSingle(obj), q.y, q.z, q.w);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        1 => (string.Empty, new NumberLimitParam(property.quaternionValue.y), (Action<object>)(obj =>
                        {
                            Quaternion q = property.quaternionValue;
                            property.quaternionValue = new Quaternion(q.x, Convert.ToSingle(obj), q.z, q.w);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        2 => (string.Empty, new NumberLimitParam(property.quaternionValue.z), (Action<object>)(obj =>
                        {
                            Quaternion q = property.quaternionValue;
                            property.quaternionValue = new Quaternion(q.x, q.y, Convert.ToSingle(obj), q.w);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        3 => (string.Empty, new NumberLimitParam(property.quaternionValue.w), (Action<object>)(obj =>
                        {
                            Quaternion q = property.quaternionValue;
                            property.quaternionValue = new Quaternion(q.x, q.y, q.z, Convert.ToSingle(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for quaternion type", default, null),
                    };
                case SerializedPropertyType.Color:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.colorValue.r), (Action<object>)(obj =>
                        {
                            Color c = property.colorValue;
                            property.colorValue = new Color(Convert.ToSingle(obj), c.g, c.b, c.a);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        1 => (string.Empty, new NumberLimitParam(property.colorValue.g), (Action<object>)(obj =>
                        {
                            Color c = property.colorValue;
                            property.colorValue = new Color(c.r, Convert.ToSingle(obj), c.b, c.a);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        2 => (string.Empty, new NumberLimitParam(property.colorValue.b), (Action<object>)(obj =>
                        {
                            Color c = property.colorValue;
                            property.colorValue = new Color(c.r, c.g, Convert.ToSingle(obj), c.a);
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        3 => (string.Empty, new NumberLimitParam(property.colorValue.a), (Action<object>)(obj =>
                        {
                            Color c = property.colorValue;
                            property.colorValue = new Color(c.r, c.g, c.b, Convert.ToSingle(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for color type", default, null),
                    };
            }

            string vecName;
            switch (index)
            {
                case 0:
                    vecName = "x";
                    break;
                case 1:
                    vecName = "y";
                    break;
                case 2:
                    vecName = "z";
                    break;
                case 3:
                    vecName = "w";
                    break;
                default:
                    return ($"Not supported range {index}", default, null);
            }

            SerializedProperty subProp = property.FindPropertyRelative(vecName);

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (subProp == null)
            {
                return ($"No matched prop found", default, null);
            }

            return ParsePropertyDirect(subProp);
        }

        private static void CheckLimitAndApply(NumberLimitParam sourceNumber, NumberLimitParam requiredLimit, Action<object> apply)
        {
            switch (sourceNumber.SourceType)
            {
                case SourceType.Long:
                    switch (requiredLimit.SourceType)
                    {
                        case SourceType.Long:
                            if (sourceNumber.LongV > requiredLimit.LongV)
                            {
                                apply(requiredLimit.LongV);
                            }
                            break;
                        case SourceType.Ulong:
                            if (sourceNumber.LongV >= 0 && (ulong)sourceNumber.LongV > requiredLimit.UlongV)
                            {
                                apply(requiredLimit.UlongV);
                            }
                            break;
                        case SourceType.Double:
                            if (sourceNumber.LongV > requiredLimit.DoubleV)
                            {
                                apply(requiredLimit.DoubleV);
                            }
                            break;
                        case SourceType.Decimal:
                            if (sourceNumber.LongV > requiredLimit.DecimalV)
                            {
                                apply(requiredLimit.DecimalV);
                            }
                            break;
                        case SourceType.NotSupported:
                        case SourceType.NoLimit:
                        case SourceType.Callback:
                        default:
                            return;
                    }

                    break;
                case SourceType.Ulong:
                    switch (requiredLimit.SourceType)
                    {
                        case SourceType.Long:
                            // ulong source vs long limit: if limit < 0, source > limit always; otherwise compare as ulong
                            if (requiredLimit.LongV < 0 || sourceNumber.UlongV > (ulong)requiredLimit.LongV)
                            {
                                apply(requiredLimit.LongV);
                            }
                            break;
                        case SourceType.Ulong:
                            if (sourceNumber.UlongV > requiredLimit.UlongV)
                            {
                                apply(requiredLimit.UlongV);
                            }
                            break;
                        case SourceType.Double:
                            if (sourceNumber.UlongV > requiredLimit.DoubleV)
                            {
                                apply(requiredLimit.DoubleV);
                            }
                            break;
                        case SourceType.Decimal:
                            if (sourceNumber.UlongV > requiredLimit.DecimalV)
                            {
                                apply(requiredLimit.DecimalV);
                            }
                            break;
                        case SourceType.NotSupported:
                        case SourceType.NoLimit:
                        case SourceType.Callback:
                        default:
                            return;
                    }

                    break;
                case SourceType.Double:
                    switch (requiredLimit.SourceType)
                    {
                        case SourceType.Long:
                            if (sourceNumber.DoubleV > requiredLimit.LongV)
                            {
                                apply(requiredLimit.LongV);
                            }
                            break;
                        case SourceType.Ulong:
                            if (sourceNumber.DoubleV > requiredLimit.UlongV)
                            {
                                apply(requiredLimit.UlongV);
                            }
                            break;
                        case SourceType.Double:
                            if (sourceNumber.DoubleV > requiredLimit.DoubleV)
                            {
                                apply(requiredLimit.DoubleV);
                            }
                            break;
                        case SourceType.Decimal:
                            if ((decimal)sourceNumber.DoubleV > requiredLimit.DecimalV)
                            {
                                apply(requiredLimit.DecimalV);
                            }
                            break;
                        case SourceType.NotSupported:
                        case SourceType.NoLimit:
                        case SourceType.Callback:
                        default:
                            return;
                    }

                    break;
                case SourceType.Decimal:
                    switch (requiredLimit.SourceType)
                    {
                        case SourceType.Long:
                            if (sourceNumber.DecimalV > requiredLimit.LongV)
                            {
                                apply(requiredLimit.LongV);
                            }
                            break;
                        case SourceType.Ulong:
                            if (sourceNumber.DecimalV > requiredLimit.UlongV)
                            {
                                apply(requiredLimit.UlongV);
                            }
                            break;
                        case SourceType.Double:
                            if (sourceNumber.DecimalV > (decimal)requiredLimit.DoubleV)
                            {
                                apply(requiredLimit.DoubleV);
                            }
                            break;
                        case SourceType.Decimal:
                            if (sourceNumber.DecimalV > requiredLimit.DecimalV)
                            {
                                apply(requiredLimit.DecimalV);
                            }
                            break;
                        case SourceType.NotSupported:
                        case SourceType.NoLimit:
                        case SourceType.Callback:
                        default:
                            return;
                    }

                    break;
                case SourceType.NotSupported:
                case SourceType.NoLimit:
                case SourceType.Callback:
                default:
                    return;
            }
        }

    }
}
#endif
