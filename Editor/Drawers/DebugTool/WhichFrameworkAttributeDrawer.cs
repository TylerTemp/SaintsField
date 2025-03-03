using System.Reflection;
using SaintsField.DebugTool;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DebugTool
{
    [CustomPropertyDrawer(typeof(WhichFrameworkAttribute))]
    public class WhichFrameworkAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI

        private const string ImGuiContent = "You're using <color=red>IMGUI</color>.\n\nIMGUI is supported by SaintsField, but has a bit lower priority than UI Toolkit. Feel free to submit an issue if you have any problem.";

        // private bool _overrideMessageType;
        // private EMessageType _messageType;

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return ImGuiHelpBox.GetHeight(ImGuiContent, width, MessageType.Warning);
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            return ImGuiHelpBox.Draw(position, ImGuiContent, MessageType.Warning);
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox
            {
                text = "You're using <color=green>UI Toolkit</color>. Nice.",
                messageType = HelpBoxMessageType.Info,
            };
        }
        #endregion

#endif
    }
}
