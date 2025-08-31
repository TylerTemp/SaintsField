using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ShowImageDrawer
{
    public partial class ShowImageAttributeDrawer
    {
        private Texture2D _originTexture;
        private string _error = "";

        private (Texture2D, float preferredWidth, float preferredHeight) GetPreview(SerializedProperty property,
            int maxWidth, int maxHeight, float viewWidth, string name, FieldInfo info, object parent)
        {
            // Debug.Log($"viewWidth={viewWidth}");
            if (viewWidth - 1f < Mathf.Epsilon)
            {
                return (null, maxWidth, maxWidth);
            }

            (string error, Texture2D image) = GetImage(property, name, info, parent);
            _error = error;

            if (_error != "")
            {
                return (null, 0, 0);
            }

            _originTexture = image;

            if (_originTexture == null)
            {
                return (null, 0, 0);
            }

            bool widthOk = maxWidth == -1
                ? _originTexture.width <= viewWidth
                : _originTexture.width <= maxWidth;
            bool heightOk = maxHeight == -1 || _originTexture.height <= maxHeight;

            if (widthOk && heightOk) // original width & smaller than view width
            {
                return (_originTexture, _originTexture.width, _originTexture.height);
            }

            // fixed width / overflow height
            (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), maxWidth, maxHeight,
                _originTexture.width, _originTexture.height);
            return (_originTexture, scaleWidth, scaleHeight);
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return ((ShowImageAttribute)saintsAttribute).Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            if (_error != "")
            {
                return 0;
            }

            return ((ShowImageAttribute)saintsAttribute).Above
                ? GetImageHeight(property, width, saintsAttribute, info, parent)
                : 0;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (_error != "")
            {
                return position;
            }

            return Draw(position, property, saintsAttribute, info, parent);
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            // Debug.Log($"{((ShowImageAttribute)saintsAttribute).Above}/{_error}");
            bool willDrawBelow = !((ShowImageAttribute)saintsAttribute).Above || _error != "";
            // Debug.Log($"WillDrawBelow={willDrawBelow}");
            return willDrawBelow;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            // Debug.Log($"draw below view width: {width}");

            if (_error != "")
            {
                return ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
            }

            float height = ((ShowImageAttribute)saintsAttribute).Above
                ? 0
                : GetImageHeight(property, width, saintsAttribute, info, parent);
            // Debug.Log($"GetBlowExtraHeight={height}");
            return height;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if (_error != "")
            {
                return ImGuiHelpBox.Draw(position, _error, MessageType.Error);
            }

            // EditorGUI.DrawRect(position, Color.blue);
            // Debug.Log($"DrawBelow height: {position.height}");

            return Draw(position, property, saintsAttribute, info, parent);
        }

        private float GetImageHeight(SerializedProperty property, float width, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            int maxWidth = showImageAttribute.MaxWidth;
            // int viewWidth = Mathf.FloorToInt(width);
            // int useWidth = maxWidth == -1? viewWidth: Mathf.Min(maxWidth, viewWidth);
            int maxHeight = showImageAttribute.MaxHeight;

            (Texture2D _, float _, float preferredHeight) = GetPreview(property, maxWidth, maxHeight, width,
                showImageAttribute.ImageCallback, info, parent);

            // Debug.Log($"GetImageHeight viewWidth={width} -> {preferredHeight}");
            // if (previewTexture)
            // {
            //     Debug.Log($"get preview {previewTexture.width}x{previewTexture.height}");
            // }
            // else
            // {
            //     Debug.Log($"get no preview");
            // }
            // ReSharper disable once Unity.NoNullPropagation
            return preferredHeight;
        }

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info,
            object parent)
        {
            // Debug.Log($"Draw height: {position.height}");

            if (position.height <= 0)
            {
                // Debug.Log($"height<0: {position.height}");
                return position;
            }

            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            int maxWidth = showImageAttribute.MaxWidth;
            // int useWidth = maxWidth == -1? Mathf.FloorToInt(position.width): Mathf.Min(maxWidth, Mathf.FloorToInt(position.width));
            // int maxHeight = Mathf.Min(assetPreviewAttribute.MaxHeight, Mathf.FloorToInt(position.height));
            int maxHeight = showImageAttribute.MaxHeight;

            // Debug.Log($"Draw height={position.height}");

            (Texture2D previewTexture, float preferredWidth, float preferredHeight) = GetPreview(property, maxWidth,
                maxHeight, position.width, showImageAttribute.ImageCallback, info, parent);

            // Debug.Log($"preview to {preferredWidth}x{preferredHeight}");
            // if (previewTexture)
            // {
            //     Debug.Log($"draw get preview {previewTexture.width}x{previewTexture.height}");
            // }
            // else
            // {
            //     Debug.Log($"draw get no preview");
            // }

            if (previewTexture == null)
            {
                return position;
            }

            EAlign align = showImageAttribute.Align;
            float xOffset;
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (align)
            {
                case EAlign.Start:
                    xOffset = 0;
                    break;
                case EAlign.Center:
                    xOffset = (position.width - preferredWidth) / 2;
                    break;
                case EAlign.End:
                    xOffset = position.width - preferredWidth;
                    break;
                case EAlign.FieldStart:
                    xOffset = EditorGUIUtility.labelWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(align), align, null);
            }

            Rect previewRect = new Rect(position)
            {
                x = position.x + xOffset,
                height = preferredHeight,
                width = preferredWidth,
            };

            // EditorGUI.DrawRect(previewRect, Color.blue);

            GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);

            // GUI.Label(previewRect, previewTexture);
            // GUI.Label(previewRect, new GUIContent(previewTexture));

            return RectUtils.SplitHeightRect(position, preferredHeight).leftRect;
        }
    }
}
