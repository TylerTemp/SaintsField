using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GameObjectActiveAttribute))]
    public class GameObjectActiveAttributeDrawer: DecToggleAttributeDrawer
    {
        private const string SeeXml = "<color=white><icon='eye.png' /></color>";
        private const string UnSeeXml = "<color=orange><icon='eye-slash.png' /></color>";

        private float _width = -1;

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            if (_width >= 0)
            {
                return _width;
            }
            // object target = property.serializedObject.targetObject;
            // string labelXml = GetButtonLabelXml((DecButtonAttribute)saintsAttribute, target, target.GetType());
            float xmlWidth = RichTextDrawer.GetWidth(label, position.height, RichTextDrawer.ParseRichXml(UnSeeXml, ""));
            if (xmlWidth > 0)
            {
                return _width = xmlWidth;
            }

            return position.height;
        }

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            _error = "";

            GameObject go;
            if (property.objectReferenceValue is GameObject isGO)
            {
                go = isGO;
            }
            else
            {
                go = ((Component) property.objectReferenceValue)?.gameObject;
            }

            bool goIsNull = go == null;
            bool goActive = !goIsNull && go.activeSelf;

            Draw(position, property, label, goActive? SeeXml: UnSeeXml, goActive, (newIsActive) =>
            {
                if (go != null)
                {
                    go.SetActive(newIsActive);
                }
            });

            if (goIsNull)
            {
                _error = $"Unable to get GameObject from {property.name}";
            }
            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            _error == ""
                ? position
                : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
