using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AssetPreviewDrawer
{
    public partial class AssetPreviewAttributeDrawer
    {
        private Texture2D _previewTexture;
        // private int _cachedWidth;
        // private int _cachedHeight;
        // private Texture2D _cachedWidthTexture;

        ~AssetPreviewAttributeDrawer()
        {
            _previewTexture = null;
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return ((AssetPreviewAttribute)saintsAttribute).Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = MismatchError(property, info, parent);
            if (error != null)
            {
                return 0;
            }

            return ((AssetPreviewAttribute)saintsAttribute).Above
                ? GetExtraHeight(property, width, saintsAttribute, info, parent)
                : 0;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            string error = MismatchError(property, info, parent);
            if (error != null)
            {
                return position;
            }

            return Draw(position, property, saintsAttribute, info, parent);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            if (MismatchError(property, info, parent) != null)
            {
                return true;
            }

            return !((AssetPreviewAttribute)saintsAttribute).Above;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = MismatchError(property, info, parent);
            if (error != null)
            {
                return ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            return ((AssetPreviewAttribute)saintsAttribute).Above
                ? 0
                : GetExtraHeight(property, width, saintsAttribute, info, parent);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = MismatchError(property, info, parent);
            if (error != null)
            {
                return ImGuiHelpBox.Draw(position, error, MessageType.Error);
            }

            return Draw(position, property, saintsAttribute, info, parent);
        }

        private float GetExtraHeight(SerializedProperty property, float width, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            // int maxWidth = assetPreviewAttribute.Width;
            // // int useWidth = maxWidth == -1? Mathf.FloorToInt(width): Mathf.Min(maxWidth, Mathf.FloorToInt(width));
            // int maxHeight = assetPreviewAttribute.Height;

            if (width - 1 < Mathf.Epsilon)
            {
                return 0;
            }

            Texture2D previewTexture = GetPreview(GetCurObject(property, info, parent));
            if (previewTexture == null)
            {
                return 0;
            }

            float useWidth = assetPreviewAttribute.Align == EAlign.FieldStart
                ? Mathf.Max(width - EditorGUIUtility.labelWidth, 1)
                : width;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ASSET_PREVIEW
            Debug.Log($"#AssetPreview# useWidth={useWidth}, width={width}, labelWidth={EditorGUIUtility.labelWidth}, Align={assetPreviewAttribute.Align}");
#endif

            (int width, int height) size =
                Tex.GetProperScaleRect(
                    Mathf.FloorToInt(useWidth),
                    assetPreviewAttribute.Width, assetPreviewAttribute.Height,
                    previewTexture.width, previewTexture.height);
            // GetPreviewScaleSize(maxWidth, maxHeight, useWidth, previewTexture);
            // ReSharper disable once Unity.NoNullPropagation
            float result = size.height;
            // Debug.Log($"GetExtraHeight: {result}");
            return result;
        }

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info,
            object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            // int maxWidth = assetPreviewAttribute.Width;
            // // int useWidth = maxWidth == -1? Mathf.FloorToInt(position.width): Mathf.Min(maxWidth, Mathf.FloorToInt(position.width));
            // // int maxHeight = Mathf.Min(assetPreviewAttribute.MaxHeight, Mathf.FloorToInt(position.height));
            // int maxHeight = assetPreviewAttribute.Height;

            // return position;

            if (position.width - 1 < Mathf.Epsilon)
            {
                return position;
            }

            Texture2D previewTexture = GetPreview(GetCurObject(property, info, parent));

            if (previewTexture == null || previewTexture.width == 1)
            {
                return position;
            }

            // Debug.Log($"render texture {previewTexture.width}x{previewTexture.height} @ {position}");

            float useWidth = assetPreviewAttribute.Align == EAlign.FieldStart
                ? Mathf.Max(position.width - EditorGUIUtility.labelWidth, 1)
                : position.width;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ASSET_PREVIEW
            Debug.Log($"useWidth={useWidth}, width={position.width}, labelWidth={EditorGUIUtility.labelWidth}, Align={assetPreviewAttribute.Align}");
#endif

            (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(
                Mathf.FloorToInt(useWidth),
                assetPreviewAttribute.Width, assetPreviewAttribute.Height,
                previewTexture.width, previewTexture.height);
            // GetPreviewScaleSize(maxWidth, maxHeight, useWidth, previewTexture);

            EAlign align = assetPreviewAttribute.Align;
            float xOffset;
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (align)
            {
                case EAlign.Start:
                    xOffset = 0;
                    break;
                case EAlign.Center:
                    xOffset = (position.width - scaleWidth) / 2;
                    break;
                case EAlign.End:
                    xOffset = position.width - scaleWidth;
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
                height = scaleHeight,
                width = scaleWidth,
            };

            // GUI.Label(previewRect, previewTexture);
            GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);

            // EditorGUI.DrawRect(previewRect, Color.blue);

            Rect leftOutRect = RectUtils.SplitHeightRect(position, scaleHeight).leftRect;
            // Debug.Log($"leftOutRect={leftOutRect}");

            // return leftOutRect;
            return leftOutRect;
        }
    }
}
