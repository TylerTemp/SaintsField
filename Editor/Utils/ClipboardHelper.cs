using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class ClipboardHelper
    {
        public static bool CanCopySerializedProperty(SerializedPropertyType propertyType)
        {
            if (IsGeneralType(propertyType))
            {
                return EnsureHasSerializedPropertyMethod() && EnsureSetSerializedPropertyMethod();
            }

            PropertyInfo propertyInfo = EnsurePropertyInfo(propertyType);

            // Debug.Log($"{property.propertyType}/{propertyInfo}");

            return propertyInfo != null && propertyInfo.CanWrite;
        }

        public static void DoCopySerializedProperty(SerializedProperty property)
        {
            if (IsGeneralType(property.propertyType))
            {
                // Debug.Log($"SetSerializedProperty {property.propertyPath}/{property.propertyType}");
                SetSerializedProperty(property);
                return;
            }

            PropertyInfo propertyInfo = EnsurePropertyInfo(property.propertyType);
            if (propertyInfo == null || !propertyInfo.CanWrite)
            {
                return;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    propertyInfo.SetValue(null, property.longValue);
                    return;
                case SerializedPropertyType.Boolean:
                    propertyInfo.SetValue(null, property.boolValue);
                    return;
                case SerializedPropertyType.Float:
                    propertyInfo.SetValue(null, property.floatValue);
                    return;
                case SerializedPropertyType.String:
                case SerializedPropertyType.Character:
                    propertyInfo.SetValue(null, property.stringValue);
                    return;
                case SerializedPropertyType.Color:
                    propertyInfo.SetValue(null, property.colorValue);
                    return;
                case SerializedPropertyType.ObjectReference:
                    propertyInfo.SetValue(null, property.objectReferenceValue);
                    return;
                case SerializedPropertyType.Vector2:
                    propertyInfo.SetValue(null, property.vector2Value);
                    return;
                case SerializedPropertyType.Vector3:
                    propertyInfo.SetValue(null, property.vector3Value);
                    return;
                case SerializedPropertyType.Vector4:
                    propertyInfo.SetValue(null, property.vector4Value);
                    return;
                case SerializedPropertyType.Rect:
                    propertyInfo.SetValue(null, property.rectValue);
                    return;
                case SerializedPropertyType.AnimationCurve:
                    propertyInfo.SetValue(null, property.animationCurveValue);
                    return;
                case SerializedPropertyType.Bounds:
                    propertyInfo.SetValue(null, property.boundsValue);
                    return;
                case SerializedPropertyType.Quaternion:
                    propertyInfo.SetValue(null, property.quaternionValue);
                    return;
                case SerializedPropertyType.ExposedReference:
                    return;
                case SerializedPropertyType.FixedBufferSize:
                    return;
                case SerializedPropertyType.Vector2Int:
                {
                    Vector2Int v2Int = property.vector2IntValue;
                    propertyInfo.SetValue(null, new Vector2(v2Int.x, v2Int.y));
                    return;
                }
                case SerializedPropertyType.Vector3Int:
                {
                    Vector3Int v3Int = property.vector3IntValue;
                    propertyInfo.SetValue(null, new Vector2(v3Int.x, v3Int.y));
                    return;
                }
                case SerializedPropertyType.RectInt:
                {
                    RectInt rectInt = property.rectIntValue;
                    propertyInfo.SetValue(null, new Rect(rectInt.position, rectInt.size));
                    return;
                }
                case SerializedPropertyType.BoundsInt:
                {
                    BoundsInt boundsInt = property.boundsIntValue;
                    propertyInfo.SetValue(null, new Bounds(boundsInt.position, boundsInt.size));
                    return;
                }

#if UNITY_2019_3_OR_NEWER
                case SerializedPropertyType.ManagedReference:
#endif
                case SerializedPropertyType.Generic:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType,
                        null);
                case SerializedPropertyType.Gradient:
#if UNITY_2022_1_OR_NEWER
                    propertyInfo.SetValue(null, property.gradientValue);
#endif
                    return;
#if UNITY_2021_1_OR_NEWER
                case SerializedPropertyType.Hash128:
                    propertyInfo.SetValue(null, property.hash128Value);
                    return;
#endif
                default:
                    return;
            }
        }

        public static (bool hasReflection, bool hasValue) CanPasteSerializedProperty(SerializedPropertyType propertyType)
        {
            if (IsGeneralType(propertyType))
            {
                bool reflection = EnsureHasSerializedPropertyMethod() && EnsureGetSerializedPropertyMethod();
                if (!reflection)
                {
                    return (false, false);
                }
                bool hasGenValue = HasSerializedProperty();
                return (true, hasGenValue);
            }

            // PropertyInfo propertyInfo = EnsurePropertyInfo(property.propertyType);
            PropertyInfo hasProp = EnsureHasProp(propertyType);
            if (hasProp == null)
            {
                return (false, false);
            }

            if (!hasProp.CanRead)
            {
                return (true, false);
            }
            bool hasValue = (bool)hasProp.GetValue(null);
            return (true, hasValue);
        }

        public static void DoPasteSerializedProperty(SerializedProperty property)
        {
            if (IsGeneralType(property.propertyType))
            {
                GetSerializedProperty(property);
                return;
            }

            PropertyInfo propertyInfo = EnsurePropertyInfo(property.propertyType);
            if (propertyInfo == null || !propertyInfo.CanRead)
            {
                return;
            }

            object value = propertyInfo.GetValue(null);
            switch (property.propertyType)
            {
                // case SerializedPropertyType.Generic:
                //     return EnsureGetSerializedPropertyMethod();
                case SerializedPropertyType.Integer:
                    property.longValue = (long)value;
                    return;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool)value;
                    return;
                case SerializedPropertyType.Float:
                    property.floatValue = (float)value;
                    return;
                case SerializedPropertyType.String:
                    property.stringValue = (string)value;
                    return;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color)value;
                    return;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (UnityEngine.Object)value;
                    return;
                case SerializedPropertyType.LayerMask:
                    property.longValue = (long)value;
                    return;
                case SerializedPropertyType.Enum:
                    property.longValue = (long)value;
                    return;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2)value;
                    return;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3)value;
                    return;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4)value;
                    return;
                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect)value;
                    return;
                case SerializedPropertyType.ArraySize:
                    property.longValue = (long)value;
                    return;
                case SerializedPropertyType.Character:
                {
                    string stringValue = (string)value;
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return;
                    }

                    property.stringValue = $"{stringValue[0]}";
                    return;
                }
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = (AnimationCurve)value;
                    return;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = (Bounds)value;
                    return;
                case SerializedPropertyType.Gradient:
#if UNITY_2022_1_OR_NEWER
                    property.gradientValue = (Gradient)value;
#endif
                    return;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (Quaternion)value;
                    return;
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
                    return;
                case SerializedPropertyType.Vector2Int:
                {
                    Vector2 v2 = (Vector2)value;
                    property.vector2IntValue = new Vector2Int((int)v2.x, (int)v2.y);
                    return;
                }
                case SerializedPropertyType.Vector3Int:
                {
                    Vector3 v3 = (Vector3)value;
                    property.vector3IntValue = new Vector3Int((int)v3.x, (int)v3.y, (int)v3.z);
                    return;
                }
                case SerializedPropertyType.RectInt:
                {
                    Rect rect = (Rect)value;
                    property.rectIntValue = new RectInt((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
                    return;
                }
                case SerializedPropertyType.BoundsInt:
                {
                    Bounds bounds = (Bounds)value;
                    Vector3 postion = bounds.center;
                    Vector3Int positionVector3Int = new Vector3Int((int)postion.x, (int)postion.y, (int)postion.z);
                    Vector3 size = bounds.size;
                    Vector3Int sizeVector3Int = new Vector3Int((int)size.x, (int)size.y, (int)size.z);
                    property.boundsIntValue = new BoundsInt(positionVector3Int, sizeVector3Int);
                    return;
                }
#if UNITY_2021_3_OR_NEWER
                case SerializedPropertyType.ManagedReference:
#endif
                case SerializedPropertyType.Generic:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
#if UNITY_2021_3_OR_NEWER
                case SerializedPropertyType.Hash128:
                    property.hash128Value = (Hash128)value;
                    return;
#endif
                default:
                    return;
            }
        }

        private static bool _clipboardTypeSetup;
        private static Type _clipboardType;

        private static Type GetClipboardType()
        {
            // ReSharper disable once InvertIf
            if (!_clipboardTypeSetup)
            {
                _clipboardTypeSetup = true;
                _clipboardType = Type.GetType("UnityEditor.Clipboard, UnityEditor.CoreModule");
            }

            return _clipboardType;
        }

        private static bool _hasSerializedPropertySetup;
        private static MethodInfo _hasSerializedPropertyMethodInfo;

        public static bool EnsureHasSerializedPropertyMethod()
        {
            if (!_hasSerializedPropertySetup)
            {
                _hasSerializedPropertySetup = true;
                Type clipboardType = GetClipboardType();
                if (clipboardType == null)
                {
                    return false;
                }
                _hasSerializedPropertyMethodInfo = clipboardType.GetMethod("HasSerializedProperty", BindingFlags.Public | BindingFlags.Static);
            }

            return _hasSerializedPropertyMethodInfo != null;
        }



        public static bool HasSerializedProperty()
        {
            if(EnsureHasSerializedPropertyMethod())
            {
                return (bool)_hasSerializedPropertyMethodInfo.Invoke(null, null);
            }

            return false;
        }

        private static bool _setSerializedPropertySetup;
        private static MethodInfo _setSerializedPropertyMethodInfo;

        public static bool EnsureSetSerializedPropertyMethod()
        {
            if (!_setSerializedPropertySetup)
            {
                _setSerializedPropertySetup = true;
                Type clipboardType = GetClipboardType();
                if (clipboardType == null)
                {
                    return false;
                }
                _setSerializedPropertyMethodInfo = clipboardType.GetMethod("SetSerializedProperty", BindingFlags.Public | BindingFlags.Static);
            }

            return _setSerializedPropertyMethodInfo != null;
        }

        public static void SetSerializedProperty(SerializedProperty property)
        {
            if (EnsureSetSerializedPropertyMethod())
            {
                // Debug.Log($"invoke {_setSerializedPropertyMethodInfo.Name} {string.Join(",", _setSerializedPropertyMethodInfo.GetParameters().Cast<object>())}");

                _setSerializedPropertyMethodInfo.Invoke(null, new object[] { property });
                // Debug.Log(EditorGUIUtility.systemCopyBuffer);
            }
        }

        private static bool _getSerializedPropertySetup;
        private static MethodInfo _getSerializedPropertyMethodInfo;

        public static bool EnsureGetSerializedPropertyMethod()
        {
            if (!_getSerializedPropertySetup)
            {
                _getSerializedPropertySetup = true;
                Type clipboardType = GetClipboardType();
                if (clipboardType == null)
                {
                    return false;
                }
                _getSerializedPropertyMethodInfo = clipboardType.GetMethod("GetSerializedProperty", BindingFlags.Public | BindingFlags.Static);
            }

            return _getSerializedPropertyMethodInfo != null;
        }

        public static void GetSerializedProperty(SerializedProperty property)
        {
            if (EnsureGetSerializedPropertyMethod())
            {
                _getSerializedPropertyMethodInfo.Invoke(null, new object[]{property});
            }
        }

        public static PropertyInfo EnsureHasProp(SerializedPropertyType propertyType)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (propertyType)
            {

                case SerializedPropertyType.Integer:
                    return EnsureHas("long");
                case SerializedPropertyType.Boolean:
                    return EnsureHas("bool");
                case SerializedPropertyType.Float:
                    return EnsureHas("float");
                case SerializedPropertyType.String:
                    return EnsureHas("string");
                case SerializedPropertyType.Color:
                    return EnsureHas("color");
                case SerializedPropertyType.ObjectReference:
                    return EnsureHas("object");
                case SerializedPropertyType.LayerMask:
                    return EnsureHas("layerMask");
                case SerializedPropertyType.Enum:
                    return EnsureHas("long");
                case SerializedPropertyType.Vector2:
                    return EnsureHas("vector2");
                case SerializedPropertyType.Vector3:
                    return EnsureHas("vector3");
                case SerializedPropertyType.Vector4:
                    return EnsureHas("vector4");
                case SerializedPropertyType.Rect:
                    return EnsureHas("rect");
                case SerializedPropertyType.ArraySize:
                    return EnsureHas("long");
                case SerializedPropertyType.Character:
                    return EnsureHas("string");
                case SerializedPropertyType.AnimationCurve:
                    return EnsureHas("animationCurve");
                case SerializedPropertyType.Bounds:
                    return EnsureHas("bounds");
                case SerializedPropertyType.Gradient:
                    return EnsureHas("gradient");
                case SerializedPropertyType.Quaternion:
                    return EnsureHas("quaternion");
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
                    return null;
                case SerializedPropertyType.Vector2Int:
                    return EnsureHas("vector2");
                case SerializedPropertyType.Vector3Int:
                    return EnsureHas("vector3");
                case SerializedPropertyType.RectInt:
                    return EnsureHas("rect");
                case SerializedPropertyType.BoundsInt:
                    return EnsureHas("bounds");
#if UNITY_2021_3_OR_NEWER
                case SerializedPropertyType.Hash128:
                    return EnsureHas("hash128");
                case SerializedPropertyType.ManagedReference:
#endif
                case SerializedPropertyType.Generic:
                    throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
                default:
                    // throw new ArgumentOutOfRangeException();
                    return null;
            }
        }

        public static PropertyInfo EnsurePropertyInfo(SerializedPropertyType propertyType)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (propertyType)
            {
                // case SerializedPropertyType.Generic:
                //     return EnsureGetSerializedPropertyMethod();
                case SerializedPropertyType.Integer:
                    return GetPropertyInfo("long");
                case SerializedPropertyType.Boolean:
                    return GetPropertyInfo("bool");
                case SerializedPropertyType.Float:
                    return GetPropertyInfo("float");
                case SerializedPropertyType.String:
                    return GetPropertyInfo("string");
                case SerializedPropertyType.Color:
                    return GetPropertyInfo("color");
                case SerializedPropertyType.ObjectReference:
                    return GetPropertyInfo("object");
                case SerializedPropertyType.LayerMask:
                    return GetPropertyInfo("layerMask");
                case SerializedPropertyType.Enum:
                    return GetPropertyInfo("long");
                case SerializedPropertyType.Vector2:
                    return GetPropertyInfo("vector2");
                case SerializedPropertyType.Vector3:
                    return GetPropertyInfo("vector3");
                case SerializedPropertyType.Vector4:
                    return GetPropertyInfo("vector4");
                case SerializedPropertyType.Rect:
                    return GetPropertyInfo("rect");
                case SerializedPropertyType.ArraySize:
                    return GetPropertyInfo("long");
                case SerializedPropertyType.Character:
                    return GetPropertyInfo("string");
                case SerializedPropertyType.AnimationCurve:
                    return GetPropertyInfo("animationCurve");
                case SerializedPropertyType.Bounds:
                    return GetPropertyInfo("bounds");
                case SerializedPropertyType.Gradient:
                    return GetPropertyInfo("gradient");
                case SerializedPropertyType.Quaternion:
                    return GetPropertyInfo("quaternion");
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
                    return null;
                case SerializedPropertyType.Vector2Int:
                    return GetPropertyInfo("vector2");
                case SerializedPropertyType.Vector3Int:
                    return GetPropertyInfo("vector3");
                case SerializedPropertyType.RectInt:
                    return GetPropertyInfo("rect");
                case SerializedPropertyType.BoundsInt:
                    return GetPropertyInfo("bounds");
#if UNITY_2021_3_OR_NEWER
                case SerializedPropertyType.ManagedReference:
#endif
                case SerializedPropertyType.Generic:
                    throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
#if UNITY_2021_3_OR_NEWER
                // case SerializedPropertyType.ManagedReference:
                //     return EnsureGetSerializedPropertyMethod();
                case SerializedPropertyType.Hash128:
                    return GetPropertyInfo("hash128");
#endif
                default:
                    // throw new ArgumentOutOfRangeException();
                    return null;
            }
        }

        private static bool IsGeneralType(SerializedPropertyType serializedPropertyType)
        {
#if UNITY_2021_3_OR_NEWER
            if (serializedPropertyType == SerializedPropertyType.ManagedReference)
            {
                return true;
            }
#endif

            return serializedPropertyType == SerializedPropertyType.Generic;
        }

        private static readonly Dictionary<string, PropertyInfo> HasPropertyInfos = new Dictionary<string, PropertyInfo>();

        private static PropertyInfo EnsureHas(string name)
        {
            if (HasPropertyInfos.TryGetValue(name, out PropertyInfo info))
            {
                return info;
            }

            Type clipboardType = GetClipboardType();
            if (clipboardType == null)
            {
                return HasPropertyInfos[name] = null;
            }

            PropertyInfo propertyInfo = clipboardType.GetProperty("has" + Capitalize(name), BindingFlags.Public | BindingFlags.Static);
            if (propertyInfo == null)
            {
                return HasPropertyInfos[name] = null;
            }

            return HasPropertyInfos[name] = propertyInfo;

        }

        private static readonly Dictionary<string, PropertyInfo> PropertyInfos = new Dictionary<string, PropertyInfo>();

        private static PropertyInfo GetPropertyInfo(string name)
        {
            if (PropertyInfos.TryGetValue(name, out PropertyInfo propertyInfo))
            {
                return propertyInfo;
            }

            Type clipboardType = GetClipboardType();
            if (clipboardType == null)
            {
                return null;
            }

            string propName = $"{name}Value";
            // Debug.Log(propName);
            // Debug.Log(name);

            propertyInfo = clipboardType.GetProperty(propName, BindingFlags.Public | BindingFlags.Static);
            if (propertyInfo == null)
            {
                return null;
            }

            return PropertyInfos[name] = propertyInfo;
        }

        private static string Capitalize(string s)
        {
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        // public static void Test()
        // {
        //     // Get the type of the Clipboard class using the fully qualified name
        //     Type clipboardType = Type.GetType("UnityEditor.Clipboard, UnityEditor.CoreModule");
        //
        //     if (clipboardType == null)
        //     {
        //         Console.WriteLine("Clipboard type not found.");
        //         return;
        //     }
        //
        //     // Get the hasVector3 property and call its getter
        //     PropertyInfo hasVector3Property = clipboardType.GetProperty("hasVector3", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        //     bool hasVector3 = (bool)hasVector3Property.GetValue(null);
        //     Console.WriteLine("hasVector3: " + hasVector3);
        //
        //     // Get the vector3Value property
        //     PropertyInfo vector3ValueProperty = clipboardType.GetProperty("vector3Value", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        //
        //     // Call the getter of vector3Value
        //     Vector3 vector3Value = (Vector3)vector3ValueProperty.GetValue(null);
        //     Console.WriteLine("vector3Value (getter): " + vector3Value);
        //
        //     // Call the setter of vector3Value
        //     Vector3 newVector3Value = new Vector3(1.0f, 2.0f, 3.0f);
        //     vector3ValueProperty.SetValue(null, newVector3Value);
        //     Console.WriteLine("vector3Value (setter): " + newVector3Value);
        // }
    }
}
