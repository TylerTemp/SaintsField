using System;
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

        public static (bool reflectionOk, bool result) HasSerializedProperty()
        {
            if (!_hasSerializedPropertySetup)
            {
                _hasSerializedPropertySetup = true;
                Type clipboardType = GetClipboardType();
                if (clipboardType == null)
                {
                    return (false, false);
                }
                _hasSerializedPropertyMethodInfo = clipboardType.GetMethod("HasSerializedProperty", BindingFlags.Public | BindingFlags.Static);
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_hasSerializedPropertyMethodInfo == null)
            {
                return (false, false);
            }

            return (true, (bool)_hasSerializedPropertyMethodInfo.Invoke(null, null));
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

        // private static bool _setupPropertyCopyPasteSetup;
        // private static MethodInfo _setupPropertyCopyPasteMethod;
        //
        // public static void SetupPropertyCopyPaste(
        //     SerializedProperty property,
        //     GenericMenu menu,
        //     UnityEngine.Event evt)
        // {
        //     if (!_setupPropertyCopyPasteSetup)
        //     {
        //         _setupPropertyCopyPasteSetup = true;
        //
        //         // Get the type of the internal static class
        //         Type clipboardContextMenuType = Type.GetType("UnityEditor.ClipboardContextMenu, UnityEditor.CoreModule");
        //
        //         if (clipboardContextMenuType == null)
        //         {
        //             return;
        //         }
        //
        //         // Get the method info for the internal static method
        //         MethodInfo setupPropertyCopyPasteMethod =
        //             clipboardContextMenuType.GetMethod("SetupPropertyCopyPaste",
        //                 BindingFlags.NonPublic | BindingFlags.Static);
        //
        //         if (setupPropertyCopyPasteMethod == null)
        //         {
        //             return;
        //         }
        //
        //         // Invoke the method using reflection
        //         _setupPropertyCopyPasteMethod = setupPropertyCopyPasteMethod;
        //     }
        //
        //     if (_setupPropertyCopyPasteMethod != null)
        //     {
        //         _setupPropertyCopyPasteMethod.Invoke(null, new object[] { property, menu, evt });
        //     }
        //
        // }
    }
}
