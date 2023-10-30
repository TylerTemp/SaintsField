using System;
using ExtInspector.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
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

        protected override (bool isActive, Rect position) DrawPostField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // Debug.Log($"draw below {position}");
            // return Draw(position, property, label, saintsAttribute);
            float width = _width > 0
                ? _width
                : GetPostFieldWidth(position, property, label, saintsAttribute);

            (Rect useRect, Rect leftRect) = RectUtils.SplitWidthRect(position, width);
            // <color=yellow><icon='eye-regular.png' /></color>

            // Type fieldType = SerializedUtil.GetType(property);
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

            Draw(useRect, property, label, goActive? SeeXml: UnSeeXml, goActive, (newIsActive) =>
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
            return (true, leftRect);
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : HelpBox.GetHeight(_error, width);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            _error == ""
                ? position
                : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
