#if SAINTSFIELD_DEBUG_IMGUI_DEBUGGER
using System;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public static class IMGUIDebugger
    {
        [MenuItem( "Window/Saints/IMGUI Debugger" )]
        public static void Open()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.GUIViewDebuggerWindow,UnityEditor")).Show();
        }
    }
}
#endif
