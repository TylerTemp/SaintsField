using UnityEditor;
using UnityEngine;

namespace ExtInspector.Standalone.Editor
{
    [CustomPropertyDrawer(typeof(LabelTextAttribute))]
    public class LabelTextAttributeDrawer: PropertyDrawer
    {
        private Texture _texture;
        // private EColor _textureColor;
        private string _textureName;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LabelTextAttribute attributeInfo = (LabelTextAttribute)attribute;
            if (string.IsNullOrEmpty(attributeInfo.text) && attributeInfo.icon == null)
            {
                EditorGUI.PropertyField(position, property, null);
                return;
            }

            label.text = " " + attributeInfo.text;

            bool iconHasColor = false;
            if (attributeInfo.icon != null)
            {
                if (_textureName != attributeInfo.icon)
                {
                    _textureName = attributeInfo.icon;

                    if (_texture)
                    {
                        Object.DestroyImmediate(_texture);
                        _texture = null;
                    }
                    _texture = Tex.ApplyTextureColor((Texture2D)EditorGUIUtility.Load(_textureName), attributeInfo.iconColor.GetColor());
                }

                iconHasColor = attributeInfo.iconColor != EColor.Default && attributeInfo.iconColor != EColor.White;

                label.image = _texture;
            }

            bool useCustomColor = attributeInfo.textColor != EColor.Default;
            if (useCustomColor && iconHasColor)
            {
                useCustomColor = false;
                Debug.LogWarning($"can't set color for both icon and text");
            }
            Color oldColor = GUI.contentColor;
            if(useCustomColor)
            {
                GUI.contentColor = attributeInfo.textColor.GetColor();
            }
            EditorGUI.PropertyField(position, property, label);
            if(useCustomColor)
            {
                GUI.contentColor = oldColor;
            }
        }
    }
}
