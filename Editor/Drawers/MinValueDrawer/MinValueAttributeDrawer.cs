using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.MinValueDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(MinValueAttribute), true)]
    public partial class MinValueAttributeDrawer : SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static (IReadOnlyList<string> errors, IReadOnlyList<(string message, Action fix)> checkerResults) CheckPropertyValue(SerializedProperty property, MinValueAttribute minValueAttribute, Action<object> onValueChangedCallback, MemberInfo info, object parent)
        {
            List<string> errors = new List<string>(4);
            List<(string message, Action fix)> checkerResults = new List<(string message, Action fix)>(4);
            // bool changed = false;
            for (int index = 0; index < minValueAttribute.Positions.Count; index++)
            {
                NumberLimitParam requiredLimit = minValueAttribute.Positions[index];
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

                (string message, Action fix) checker = CheckLimitAndApply(sourceNumber, requiredLimit, value =>
                {
                    apply.Invoke(value);
                    onValueChangedCallback.Invoke(value);
                });
                // if (fix != null)
                // {
                //     fix.Invoke();
                // }
                if(checker.fix != null)
                {
                    checkerResults.Add(checker);
                }
            }

            // string error = string.Join("\n", errors);
            //
            // UIToolkitUtils.SetHelpBox(helpBox, error);
            return (errors, checkerResults);
        }

        private static int ToInt32Ceiling(object obj)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (obj)
            {
                case double d:
                    return (int)Math.Ceiling(d);
                case float f:
                    return (int)Math.Ceiling(f);
                case decimal dec:
                    return (int)Math.Ceiling(dec);
                default:
                    return Convert.ToInt32(obj);
            }
        }

        private static long ToInt64Ceiling(object obj)
        {
            switch (obj)
            {
                case double d:
                    return (long)Math.Ceiling(d);
                case float f:
                    return (long)Math.Ceiling(f);
                case decimal dec:
                    return (long)Math.Ceiling(dec);
                default:
                    return Convert.ToInt64(obj);
            }
        }

        private static uint ToUInt32Ceiling(object obj)
        {
            switch (obj)
            {
                case double d:
                    return d < 0 ? 0u : (uint)Math.Ceiling(d);
                case float f:
                    return f < 0 ? 0u : (uint)Math.Ceiling(f);
                case decimal dec:
                    return dec < 0 ? 0u : (uint)Math.Ceiling(dec);
                default:
                {
                    // Convert may throw if obj is a negative signed value; clamp to 0.
                    long signed = Convert.ToInt64(obj);
                    return signed < 0 ? 0u : (uint)signed;
                }
            }
        }

        private static ulong ToUInt64Ceiling(object obj)
        {
            switch (obj)
            {
                case double d:
                    return d < 0 ? 0ul : (ulong)Math.Ceiling(d);
                case float f:
                    return f < 0 ? 0ul : (ulong)Math.Ceiling(f);
                case decimal dec:
                    return dec < 0 ? 0ul : (ulong)Math.Ceiling(dec);
                default:
                {
                    long signed;
                    try
                    {
                        signed = Convert.ToInt64(obj);
                    }
                    catch (OverflowException)
                    {
                        // probably already a ulong
                        return Convert.ToUInt64(obj);
                    }
                    return signed < 0 ? 0ul : (ulong)signed;
                }
            }
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
                    return ($"Not supported type {property.propertyType}({property.numericType})", default, null);
                case SerializedPropertyNumericType.Int8:
                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.Int16:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.Int32:  // ..., int, use int
                    return (string.Empty, new NumberLimitParam(property.intValue), obj =>
                    {
                        property.intValue = ToInt32Ceiling(obj);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                case SerializedPropertyNumericType.UInt32:  // uint, use long
                    return (string.Empty, new NumberLimitParam(property.uintValue), obj =>
                    {
                        property.uintValue = ToUInt32Ceiling(obj);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                case SerializedPropertyNumericType.Int64:  // long, use long
                    return (string.Empty, new NumberLimitParam(property.longValue), obj =>
                    {
                        property.longValue = ToInt64Ceiling(obj);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                case SerializedPropertyNumericType.UInt64: // ulong, use ulong
                    return (string.Empty, new NumberLimitParam(property.ulongValue), obj =>
                    {
                        property.ulongValue = ToUInt64Ceiling(obj);
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
                        0 => (string.Empty, new NumberLimitParam(property.vector2Value.x), obj =>
                        {
                            property.vector2Value = new Vector2(Convert.ToSingle(obj), property.vector2Value.y);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
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
                        0 => (string.Empty, new NumberLimitParam(property.vector2IntValue.x), obj =>
                        {
                            property.vector2IntValue = new Vector2Int(ToInt32Ceiling(obj), property.vector2IntValue.y);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.vector2IntValue.y), (Action<object>)(obj =>
                        {
                            property.vector2IntValue = new Vector2Int(property.vector2IntValue.x, ToInt32Ceiling(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for vector2Int type", default, null),
                    };
                case SerializedPropertyType.Vector3:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.vector3Value.x), obj =>
                        {
                            property.vector3Value = new Vector3(Convert.ToSingle(obj), property.vector3Value.y, property.vector3Value.z);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.vector3Value.y), obj =>
                        {
                            property.vector3Value = new Vector3(property.vector3Value.x, Convert.ToSingle(obj), property.vector3Value.z);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
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
                        0 => (string.Empty, new NumberLimitParam(property.vector3IntValue.x), obj =>
                        {
                            property.vector3IntValue = new Vector3Int(ToInt32Ceiling(obj), property.vector3IntValue.y, property.vector3IntValue.z);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.vector3IntValue.y), obj =>
                        {
                            property.vector3IntValue = new  Vector3Int(property.vector3IntValue.x, ToInt32Ceiling(obj), property.vector3IntValue.z);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        2 => (string.Empty, new NumberLimitParam(property.vector3IntValue.z), (Action<object>)(obj =>
                        {
                            property.vector3IntValue = new Vector3Int(property.vector3IntValue.x, property.vector3IntValue.y, ToInt32Ceiling(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for vector3Int type", default, null),
                    };
                case SerializedPropertyType.Vector4:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.vector4Value.x), obj =>
                        {
                            property.vector4Value = new Vector4(Convert.ToSingle(obj), property.vector4Value.y, property.vector4Value.z, property.vector4Value.w);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.vector4Value.y), obj =>
                        {
                            property.vector4Value = new Vector4(property.vector4Value.x, Convert.ToSingle(obj), property.vector4Value.z, property.vector4Value.w);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        2 => (string.Empty, new NumberLimitParam(property.vector4Value.z), obj =>
                        {
                            property.vector4Value = new Vector4(property.vector4Value.x, property.vector4Value.y, Convert.ToSingle(obj), property.vector4Value.w);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
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
                        0 => (string.Empty, new NumberLimitParam(property.quaternionValue.x), obj =>
                        {
                            Quaternion q = property.quaternionValue;
                            property.quaternionValue = new Quaternion(Convert.ToSingle(obj), q.y, q.z, q.w);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.quaternionValue.y), obj =>
                        {
                            Quaternion q = property.quaternionValue;
                            property.quaternionValue = new Quaternion(q.x, Convert.ToSingle(obj), q.z, q.w);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        2 => (string.Empty, new NumberLimitParam(property.quaternionValue.z), obj =>
                        {
                            Quaternion q = property.quaternionValue;
                            property.quaternionValue = new Quaternion(q.x, q.y, Convert.ToSingle(obj), q.w);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
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
                        0 => (string.Empty, new NumberLimitParam(property.colorValue.r), obj =>
                        {
                            Color c = property.colorValue;
                            property.colorValue = new Color(Convert.ToSingle(obj), c.g, c.b, c.a);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.colorValue.g), obj =>
                        {
                            Color c = property.colorValue;
                            property.colorValue = new Color(c.r, Convert.ToSingle(obj), c.b, c.a);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        2 => (string.Empty, new NumberLimitParam(property.colorValue.b), obj =>
                        {
                            Color c = property.colorValue;
                            property.colorValue = new Color(c.r, c.g, Convert.ToSingle(obj), c.a);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        3 => (string.Empty, new NumberLimitParam(property.colorValue.a), (Action<object>)(obj =>
                        {
                            Color c = property.colorValue;
                            property.colorValue = new Color(c.r, c.g, c.b, Convert.ToSingle(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for color type", default, null),
                    };
                case SerializedPropertyType.Rect:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.rectValue.x), obj =>
                        {
                            Rect v = property.rectValue;
                            property.rectValue = new Rect(Convert.ToSingle(obj), v.y, v.width, v.height);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.rectValue.y), obj =>
                        {
                            Rect v = property.rectValue;
                            property.rectValue = new Rect(v.x, Convert.ToSingle(obj), v.width, v.height);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        2 => (string.Empty, new NumberLimitParam(property.rectValue.width), obj =>
                        {
                            Rect v = property.rectValue;
                            property.rectValue = new Rect(v.x, v.y, Convert.ToSingle(obj), v.height);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        3 => (string.Empty, new NumberLimitParam(property.rectValue.height), (Action<object>)(obj =>
                        {
                            Rect v = property.rectValue;
                            property.rectValue = new Rect(v.x, v.y, v.width, Convert.ToSingle(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for rect type", default, null),
                    };
                case SerializedPropertyType.RectInt:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.rectIntValue.x), obj =>
                        {
                            RectInt v = property.rectIntValue;
                            property.rectIntValue = new RectInt(ToInt32Ceiling(obj), v.y, v.width, v.height);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.rectIntValue.y), obj =>
                        {
                            RectInt v = property.rectIntValue;
                            property.rectIntValue = new RectInt(v.x, ToInt32Ceiling(obj), v.width, v.height);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        2 => (string.Empty, new NumberLimitParam(property.rectIntValue.width), obj =>
                        {
                            RectInt v = property.rectIntValue;
                            property.rectIntValue = new RectInt(v.x, v.y, ToInt32Ceiling(obj), v.height);
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        3 => (string.Empty, new NumberLimitParam(property.rectIntValue.height), (Action<object>)(obj =>
                        {
                            RectInt v = property.rectIntValue;
                            property.rectIntValue = new RectInt(v.x, v.y, v.width, ToInt32Ceiling(obj));
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for rectInt type", default, null),
                    };
                case SerializedPropertyType.Bounds:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.boundsValue.center.x), obj =>
                        {
                            property.boundsValue = new Bounds {
                                center = new Vector3(Convert.ToSingle(obj), property.boundsValue.center.y, property.boundsValue.center.z),
                                extents = property.boundsValue.extents,
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.boundsValue.center.y), obj =>
                        {
                            property.boundsValue = new Bounds {
                                center = new Vector3(property.boundsValue.center.x, Convert.ToSingle(obj), property.boundsValue.center.z),
                                extents = property.boundsValue.extents,
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        2 => (string.Empty, new NumberLimitParam(property.boundsValue.center.z), obj =>
                        {
                            property.boundsValue = new Bounds {
                                center = new Vector3(property.boundsValue.center.x, property.boundsValue.center.y, Convert.ToSingle(obj)),
                                extents = property.boundsValue.extents,
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        3 => (string.Empty, new NumberLimitParam(property.boundsValue.extents.x), obj =>
                        {
                            property.boundsValue = new Bounds
                            {
                                center = property.boundsValue.center,
                                extents = new Vector3(Convert.ToSingle(obj), property.boundsValue.extents.y, property.boundsValue.extents.z),
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        4 => (string.Empty, new NumberLimitParam(property.boundsValue.extents.y), obj =>
                        {
                            property.boundsValue = new Bounds
                            {
                                center = property.boundsValue.center,
                                extents = new Vector3(property.boundsValue.extents.x, Convert.ToSingle(obj), property.boundsValue.extents.z),
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        5 => (string.Empty, new NumberLimitParam(property.boundsValue.extents.z), (Action<object>)(obj =>
                        {
                            property.boundsValue = new Bounds
                            {
                                center = property.boundsValue.center,
                                extents = new Vector3(property.boundsValue.extents.x, property.boundsValue.extents.y, Convert.ToSingle(obj)),
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for bounds type", default, null),
                    };
                case SerializedPropertyType.BoundsInt:
                    return index switch
                    {
                        0 => (string.Empty, new NumberLimitParam(property.boundsIntValue.position.x), obj =>
                        {
                            property.boundsIntValue = new BoundsInt {
                                position = new Vector3Int(ToInt32Ceiling(obj), property.boundsIntValue.position.y, property.boundsIntValue.position.z),
                                size =property.boundsIntValue.size,
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        1 => (string.Empty, new NumberLimitParam(property.boundsIntValue.position.y), obj =>
                        {
                            property.boundsIntValue = new BoundsInt {
                                position =new Vector3Int(property.boundsIntValue.position.x, ToInt32Ceiling(obj), property.boundsIntValue.position.z),
                                size =property.boundsIntValue.size,
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        2 => (string.Empty, new NumberLimitParam(property.boundsIntValue.position.z), obj =>
                        {
                            property.boundsIntValue = new BoundsInt {
                                position =new Vector3Int(property.boundsIntValue.position.x, property.boundsIntValue.position.y, ToInt32Ceiling(obj)),
                                size =property.boundsIntValue.size,
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        3 => (string.Empty, new NumberLimitParam(property.boundsIntValue.size.x), obj =>
                        {
                            property.boundsIntValue = new BoundsInt
                            {
                                position =property.boundsIntValue.position,
                                size =new Vector3Int(ToInt32Ceiling(obj), property.boundsIntValue.size.y, property.boundsIntValue.size.z),
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        4 => (string.Empty, new NumberLimitParam(property.boundsIntValue.size.y), obj =>
                        {
                            property.boundsIntValue = new BoundsInt
                            {
                                position =property.boundsIntValue.position,
                                size =new Vector3Int(property.boundsIntValue.size.x, ToInt32Ceiling(obj), property.boundsIntValue.size.z),
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        }),
                        5 => (string.Empty, new NumberLimitParam(property.boundsIntValue.size.z), (Action<object>)(obj =>
                        {
                            property.boundsIntValue = new BoundsInt
                            {
                                position =property.boundsIntValue.position,
                                size =new Vector3Int(property.boundsIntValue.size.x, property.boundsIntValue.size.y, ToInt32Ceiling(obj)),
                            };
                            property.serializedObject.ApplyModifiedProperties();
                        })),
                        _ => ($"Out of bounds {index} for boundsInt type", default, null),
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

        private static (string message, Action fix) CheckLimitAndApply(NumberLimitParam sourceNumber, NumberLimitParam requiredLimit, Action<object> apply)
        {
            switch (sourceNumber.SourceType)
            {
                case SourceType.Long:
                    switch (requiredLimit.SourceType)
                    {
                        case SourceType.Long:
                            if (sourceNumber.LongV < requiredLimit.LongV)
                            {
                                return ($"Min {requiredLimit}, get {sourceNumber}", () => apply(requiredLimit.LongV));
                            }
                            break;
                        case SourceType.Ulong:
                            if (sourceNumber.LongV >= 0 && (ulong)sourceNumber.LongV < requiredLimit.UlongV)
                            {
                                return ($"Min {requiredLimit}, get {sourceNumber}", () => apply(requiredLimit.LongV));
                            }
                            break;
                        case SourceType.Double:
                        {
                            double ceilingedLimit = Math.Ceiling(requiredLimit.DoubleV);
                            if (sourceNumber.LongV < ceilingedLimit)
                            {
                                return ($"Min {ceilingedLimit}, get {sourceNumber.LongV}", () => apply(ceilingedLimit));
                            }
                            break;
                        }
                        case SourceType.Decimal:
                        {
                            decimal ceilingedLimit = Math.Ceiling(requiredLimit.DecimalV);
                            if (sourceNumber.LongV < ceilingedLimit)
                            {
                                return ($"Min {ceilingedLimit}, get  {sourceNumber.LongV}", () => apply(ceilingedLimit));
                            }
                            break;
                        }
                        case SourceType.NotSupported:
                        case SourceType.NoLimit:
                        case SourceType.Callback:
                        default:
                            return ("", null);
                    }

                    break;
                case SourceType.Ulong:
                    switch (requiredLimit.SourceType)
                    {
                        case SourceType.Long:
                            if (requiredLimit.LongV < 0 || sourceNumber.UlongV < (ulong)requiredLimit.LongV)
                            {
                                return ($"Min {requiredLimit.LongV}, get {sourceNumber.UlongV}",
                                    () => apply(requiredLimit.LongV));
                            }
                            break;
                        case SourceType.Ulong:
                            if (sourceNumber.UlongV < requiredLimit.UlongV)
                            {
                                return ($"Min {requiredLimit.UlongV}, get {sourceNumber.UlongV}",
                                    () => apply(requiredLimit.UlongV));
                            }
                            break;
                        case SourceType.Double:
                        {
                            // Source is unsigned integer; ceiling the limit. If limit is negative, clamp to 0.
                            double ceilingedLimit = requiredLimit.DoubleV < 0 ? 0 : Math.Ceiling(requiredLimit.DoubleV);
                            if (sourceNumber.UlongV < ceilingedLimit)
                            {
                                apply(ceilingedLimit);
                            }
                            break;
                        }
                        case SourceType.Decimal:
                        {
                            decimal ceilingedLimit = requiredLimit.DecimalV < 0 ? 0m : Math.Ceiling(requiredLimit.DecimalV);
                            if (sourceNumber.UlongV < ceilingedLimit)
                            {
                                return ($"Min  {ceilingedLimit}, get {sourceNumber.UlongV}", () => apply(ceilingedLimit));
                            }
                            break;
                        }
                        case SourceType.NotSupported:
                        case SourceType.NoLimit:
                        case SourceType.Callback:
                        default:
                            return ("", null);
                    }

                    break;
                case SourceType.Double:
                    switch (requiredLimit.SourceType)
                    {
                        case SourceType.Long:
                            if (sourceNumber.DoubleV < requiredLimit.LongV)
                            {
                                return ($"Min {requiredLimit.LongV}, get {sourceNumber.DoubleV}", ()  => apply(requiredLimit.LongV));
                            }
                            break;
                        case SourceType.Ulong:
                            if (sourceNumber.DoubleV < requiredLimit.UlongV)
                            {
                                return ($"Min {requiredLimit.UlongV}, get {sourceNumber.DoubleV}", () => apply(requiredLimit.UlongV));
                            }
                            break;
                        case SourceType.Double:
                            if (sourceNumber.DoubleV < requiredLimit.DoubleV)
                            {
                                return ($"Min {requiredLimit}, get {sourceNumber}", () => apply(requiredLimit.DoubleV));
                            }
                            break;
                        case SourceType.Decimal:
                            if ((decimal)sourceNumber.DoubleV < requiredLimit.DecimalV)
                            {
                                return ($"Min {requiredLimit}, get {sourceNumber}", () => apply(requiredLimit.DecimalV));
                            }
                            break;
                        case SourceType.NotSupported:
                        case SourceType.NoLimit:
                        case SourceType.Callback:
                        default:
                            return ("", null);
                    }

                    break;
                case SourceType.Decimal:
                    switch (requiredLimit.SourceType)
                    {
                        case SourceType.Long:
                            if (sourceNumber.DecimalV < requiredLimit.LongV)
                            {
                                return ($"Min {requiredLimit}, get {sourceNumber}", () => apply(requiredLimit.LongV));
                            }
                            break;
                        case SourceType.Ulong:
                            if (sourceNumber.DecimalV < requiredLimit.UlongV)
                            {
                                return ($"Min {requiredLimit}, get {sourceNumber}", () => apply(requiredLimit.UlongV));
                            }
                            break;
                        case SourceType.Double:
                            if (sourceNumber.DecimalV < (decimal)requiredLimit.DoubleV)
                            {
                                return ($"Min {requiredLimit}, get {sourceNumber}", () => apply(requiredLimit.DoubleV));
                            }
                            break;
                        case SourceType.Decimal:
                            if (sourceNumber.DecimalV < requiredLimit.DecimalV)
                            {
                                return ($"Min {requiredLimit}, get {sourceNumber}", () => apply(requiredLimit.DecimalV));
                            }
                            break;
                        case SourceType.NotSupported:
                        case SourceType.NoLimit:
                        case SourceType.Callback:
                        default:
                            return ("", null);
                    }

                    break;
                case SourceType.NotSupported:
                case SourceType.NoLimit:
                case SourceType.Callback:
                default:
                    break;
            }

            return ("", null);
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {

            if (property.isArray)
            {
                return null;
            }

            MinValueAttribute minValueAttribute = (MinValueAttribute) propertyAttribute;

            (IReadOnlyList<string> errors, IReadOnlyList<(string message, Action fix)> checkerResults) = CheckPropertyValue(property, minValueAttribute, _ => { }, memberInfo, parent);
            if (checkerResults.Count == 0 && errors.Count == 0)
            {
                return null;
            }

            return new AutoRunnerFixerResult
            {
                Error = string.Join("\n", checkerResults.Select(each => each.message).Concat(errors)),
                CanFix = checkerResults.Count > 0,
                Callback = () =>
                {
                    foreach ((string _, Action fix) in checkerResults)
                    {
                        fix.Invoke();
                    }
                },
            };
        }
    }
}
