using ExtInspector.Editor.Utils;
using ExtInspector.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Standalone.Editor
{
    [CustomPropertyDrawer(typeof(LabelTextAttribute))]
    public class LabelTextAttributeDrawer: PropertyDrawer
    {
        private Texture _texture;
        // private EColor _textureColor;
        // private string _textureName;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            LabelTextAttribute targetAttribute = (LabelTextAttribute)attribute;
            bool hasIcon = targetAttribute.icon != null;
            if (targetAttribute.text == null && !hasIcon)
            {
                EditorGUI.PropertyField(position, property, GUIContent.none);
                EditorGUI.EndProperty();
                return;
            }

            label.text = targetAttribute.text;

            Rect indentedPosition = EditorGUI.IndentedRect(position);

            Rect iconRect = new Rect(indentedPosition)
            {
                width = 0,
            };

            // bool iconHasColor = false;
            if (hasIcon)
            {
                // 缓存无效
                // if (_textureName != targetAttribute.icon)
                // {
                //     _textureName = targetAttribute.icon;
                //
                //     if (_texture)
                //     {
                //         Object.DestroyImmediate(_texture);
                //         _texture = null;
                //     }
                //     _texture = Tex.TextureTo((Texture2D)EditorGUIUtility.Load(_textureName), targetAttribute.iconColor.GetColor(), Mathf.FloorToInt(position.height));
                // }

                _texture = Tex.TextureTo((Texture2D)EditorGUIUtility.Load(targetAttribute.icon),
                    targetAttribute.iconColor.GetColor(), targetAttribute.iconWidth == -1? -1: targetAttribute.iconWidth, Mathf.FloorToInt(position.height));

                iconRect = new Rect(iconRect)
                {
                    width = (targetAttribute.iconWidth == -1? _texture.width: targetAttribute.iconWidth) + 2,
                };

                GUI.Label(indentedPosition, _texture);
                // GUI.DrawTexture(iconRect, _texture);
            }

            Rect textRect = new Rect(indentedPosition)
            {
                x = iconRect.x + iconRect.width,
            };
            GUIStyle labelStyle;
            if (targetAttribute.textColor == EColor.Default)
            {
                labelStyle = GUI.skin.label;
            }
            else
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        textColor = targetAttribute.textColor.GetColor(),
                    },
                };
            }
            GUI.Label(textRect, targetAttribute.text, labelStyle);

            Rect fieldRect = new Rect(position)
            {
                x = position.x + EditorGUIUtility.labelWidth,
            };

            EditorGUI.PropertyField(fieldRect, property, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
