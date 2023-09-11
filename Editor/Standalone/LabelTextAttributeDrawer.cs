using ExtInspector.Editor;
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

        private RichTextDrawer _richTextDrawer = new RichTextDrawer();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            LabelTextAttribute targetAttribute = (LabelTextAttribute)attribute;
            bool hasIcon = targetAttribute.icon != null;
            // 不使用原标签+无自定义标签=无标签区域
            // (如果单纯需要图标，则应该是text="", icon="xxx")
            if (!targetAttribute.useOldLabel && targetAttribute.text is null)
            {
                EditorGUI.PropertyField(position, property, GUIContent.none);
                EditorGUI.EndProperty();
                return;
            }

            // Debug.Log($"useOldLabel={targetAttribute.useOldLabel}/label.text={label.text}");

            string useLabelText = targetAttribute.useOldLabel
                ? label.text
                : targetAttribute.text;

            Debug.Assert(label.text is not null);

            Rect indentedPosition = EditorGUI.IndentedRect(position);

            Rect iconRect = new Rect(indentedPosition)
            {
                width = 0,
            };

            // bool iconHasColor = false;
            if (hasIcon)
            {
                // 缓存无效
                if (_texture is null || (_texture.width == 1 && _texture.height == 1))
                {
                    if (_texture)
                    {
                        Object.DestroyImmediate(_texture);
                    }

#if EXT_INSPECTOR_LOG
                    Debug.Log($"EditorGUIUtility.Load {targetAttribute.icon}");
#endif

                    _texture = Tex.TextureTo((Texture2D)EditorGUIUtility.Load(targetAttribute.icon),
                        targetAttribute.iconColor.GetColor(), targetAttribute.iconWidth == -1? -1: targetAttribute.iconWidth, Mathf.FloorToInt(position.height));
                }

                // Debug.Log(_texture);
                // Debug.Log(_texture.width);
                // Debug.Log(_texture.height);

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
            GUI.Label(textRect, useLabelText, labelStyle);

            Rect fieldRect = new Rect(position)
            {
                x = position.x + EditorGUIUtility.labelWidth,
            };

            EditorGUI.PropertyField(fieldRect, property, GUIContent.none);

            EditorGUI.EndProperty();
        }

        ~LabelTextAttributeDrawer()
        {
            if (_texture)
            {
                Object.DestroyImmediate(_texture);
            }
            _richTextDrawer.Dispose();
        }
    }
}
