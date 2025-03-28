using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public static class ClipboardHelper
    {
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

        public static bool CanCopySerializedProperty(SerializedProperty property)
        {
            return false;
        }

        public static void DoCopySerializedProperty(SerializedProperty property)
        {
        }

        public static bool CanPasteSerializedProperty(SerializedProperty property)
        {
            return false;
        }

        public static void DoPasteSerializedProperty(SerializedProperty property)
        {
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
                _setSerializedPropertyMethodInfo.Invoke(null, new object[] { property });
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

        public static bool EnsureHasProp(SerializedPropertyType propertyType)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (propertyType)
            {
                case SerializedPropertyType.Generic:
                    return EnsureHasSerializedPropertyMethod();
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
                    return false;
                case SerializedPropertyType.Vector2Int:
                    return EnsureHas("vector2");
                case SerializedPropertyType.Vector3Int:
                    return EnsureHas("vector3");
                case SerializedPropertyType.RectInt:
                    return EnsureHas("rect");
                case SerializedPropertyType.BoundsInt:
                    return EnsureHas("bounds");
                case SerializedPropertyType.ManagedReference:
                    return EnsureHasSerializedPropertyMethod();
                case SerializedPropertyType.Hash128:
                    return EnsureHas("hash128");
                default:
                    // throw new ArgumentOutOfRangeException();
                    return false;
            }
        }

        public static bool EnsurePropertyInfo(SerializedPropertyType propertyType)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (propertyType)
            {
                case SerializedPropertyType.Generic:
                    return EnsureGetSerializedPropertyMethod();
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
                    return false;
                case SerializedPropertyType.Vector2Int:
                    return EnsureHas("vector2");
                case SerializedPropertyType.Vector3Int:
                    return EnsureHas("vector3");
                case SerializedPropertyType.RectInt:
                    return EnsureHas("rect");
                case SerializedPropertyType.BoundsInt:
                    return EnsureHas("bounds");
                case SerializedPropertyType.ManagedReference:
                    return EnsureGetSerializedPropertyMethod();
                case SerializedPropertyType.Hash128:
                    return EnsureHas("hash128");
                default:
                    // throw new ArgumentOutOfRangeException();
                    return false;
            }
        }

        private static readonly Dictionary<string, PropertyInfo> HasPropertyInfos = new Dictionary<string, PropertyInfo>();

        private static bool EnsureHas(string name)
        {
            if (HasPropertyInfos.ContainsKey(name))
            {
                return true;
            }

            Type clipboardType = GetClipboardType();
            if (clipboardType == null)
            {
                return false;
            }

            PropertyInfo propertyInfo = clipboardType.GetProperty("has" + Capitalize(name), BindingFlags.Public | BindingFlags.Static);
            if (propertyInfo == null)
            {
                return false;
            }

            HasPropertyInfos[name] = propertyInfo;
            return true;

        }

        private static readonly Dictionary<string, PropertyInfo> PropertyInfos = new Dictionary<string, PropertyInfo>();

        private static bool EnsurePropertyInfo(string name)
        {
            if (PropertyInfos.ContainsKey(name))
            {
                return true;
            }

            Type clipboardType = GetClipboardType();
            if (clipboardType == null)
            {
                return false;
            }

            PropertyInfo propertyInfo = clipboardType.GetProperty(Capitalize(name), BindingFlags.Public | BindingFlags.Static);
            if (propertyInfo == null)
            {
                return false;
            }

            PropertyInfos[name] = propertyInfo;
            return true;
        }

        private static string Capitalize(string s)
        {
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static void Test()
        {
            // Get the type of the Clipboard class using the fully qualified name
            Type clipboardType = Type.GetType("UnityEditor.Clipboard, UnityEditor.CoreModule");

            if (clipboardType == null)
            {
                Console.WriteLine("Clipboard type not found.");
                return;
            }

            // Get the hasVector3 property and call its getter
            PropertyInfo hasVector3Property = clipboardType.GetProperty("hasVector3", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            bool hasVector3 = (bool)hasVector3Property.GetValue(null);
            Console.WriteLine("hasVector3: " + hasVector3);

            // Get the vector3Value property
            PropertyInfo vector3ValueProperty = clipboardType.GetProperty("vector3Value", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Call the getter of vector3Value
            Vector3 vector3Value = (Vector3)vector3ValueProperty.GetValue(null);
            Console.WriteLine("vector3Value (getter): " + vector3Value);

            // Call the setter of vector3Value
            Vector3 newVector3Value = new Vector3(1.0f, 2.0f, 3.0f);
            vector3ValueProperty.SetValue(null, newVector3Value);
            Console.WriteLine("vector3Value (setter): " + newVector3Value);
        }
    }
}
