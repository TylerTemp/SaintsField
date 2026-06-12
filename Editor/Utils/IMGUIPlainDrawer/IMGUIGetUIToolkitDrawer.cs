using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public class IMGUIGetUIToolkitDrawer: SaintsPropertyDrawer
    {
        private const string ErrorMessage =
            "No OnGUI implemention found. Are you using a UI Toolkit drawer with an IMGUI editor?";

        protected override bool UseCreateFieldIMGUI => true;

        private static string GetErrorMessage(SerializedProperty p) => $"{p.displayName}: {ErrorMessage}";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            FieldInfo info, bool hasLabelWidth, object parent)
        {
            return ImGuiHelpBox.GetHeight(GetErrorMessage(property), width, EMessageType.Error);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            ImGuiHelpBox.Draw(position, GetErrorMessage(property), EMessageType.Error);
        }
    }
}
