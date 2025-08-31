﻿#if SAINTSFIELD_DEBUG
using System;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public static class IMGUIDebugger
    {
        [MenuItem( "Saints/IMGUI Debugger" )]
        public static void Open()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.GUIViewDebuggerWindow,UnityEditor")).Show();
        }
    }
}
#endif
