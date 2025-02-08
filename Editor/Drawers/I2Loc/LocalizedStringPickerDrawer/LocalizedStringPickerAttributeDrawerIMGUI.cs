using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.I2Loc.LocalizedStringPickerDrawer
{
    public partial class LocalizedStringPickerAttributeDrawer
    {
        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static Texture2D _iconDown;


        private static GUIStyle _buttonStyle;
        private static GUIStyle ButtonStyle
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(EditorStyles.miniButton)
                    {
                        padding = new RectOffset(3, 3, 3, 3),
                    };
                }

                return _buttonStyle;
            }
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info,
            object parent)
        {
            if (!_iconDown)
            {
                _iconDown = Util.LoadResource<Texture2D>("classic-dropdown-gray.png");
            }
            if(GUI.Button(position, _iconDown, ButtonStyle))
            {
                // TODO: Open the picker
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            return MismatchError(property) != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = MismatchError(property);
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = MismatchError(property);
            return error == ""? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
