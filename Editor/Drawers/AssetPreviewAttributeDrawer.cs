using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AssetPreviewAttribute))]
    public class AssetPreviewAttributeDrawer : SaintsPropertyDrawer
    {
        private Texture2D _previewTexture;
        private int _cachedWidth;
        private int _cachedHeight;
        private Texture2D _cachedWidthTexture;

        ~AssetPreviewAttributeDrawer()
        {
            if (_previewTexture)
            {
                Object.DestroyImmediate(_previewTexture);
            }

            if (_cachedWidthTexture)
            {
                Object.DestroyImmediate(_cachedWidthTexture);
            }
        }

        private Texture2D GetPreview(int width, int maxHeight, float viewWidth, Object target)
        {
            // if (property.propertyType != SerializedPropertyType.ObjectReference ||
            //     property.objectReferenceValue == null)
            // {
            //     return null;
            // }
            // Debug.Log($"check preview {_previewTexture?.width}");
            if(_previewTexture == null || _previewTexture.width == 1)
            {
                // Debug.Log($"load preview {target}");
                _previewTexture = AssetPreview.GetAssetPreview(target);
            }

            if (_previewTexture == null || _previewTexture.width == 1)
            {
                return null;
            }

            bool widthOk = width == -1 || _previewTexture.width <= viewWidth;
            bool heightOk = maxHeight == -1 && _previewTexture.height <= maxHeight;
            if (widthOk && heightOk)
            {
                return _cachedWidthTexture = _previewTexture;
            }

            (int scaleWidth, int scaleHeight) = SaintsField.Utils.Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), width, maxHeight, _previewTexture.width, _previewTexture.height);

            if (_cachedWidth == scaleWidth && _cachedHeight == scaleHeight && _cachedWidthTexture != null && _cachedWidthTexture.width != 1 && _cachedWidthTexture.height != 1)
            {
                return _cachedWidthTexture;
            }
            _cachedWidth = scaleWidth;
            _cachedHeight = scaleHeight;
            // return _cachedWidthTexture = formatted;

            _cachedWidthTexture = SaintsField.Utils.Tex.TextureTo(_previewTexture, scaleWidth, scaleHeight);

            if (_cachedWidthTexture.width == 1)
            {
                return _previewTexture;
            }

            return _cachedWidthTexture;
        }

        protected override bool WillDrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return ((AssetPreviewAttribute)saintsAttribute).Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return 0;
            }

            return ((AssetPreviewAttribute)saintsAttribute).Above? GetExtraHeight(property, width, saintsAttribute): 0;
        }

        protected override Rect DrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return position;
            }

            return Draw(position, property, saintsAttribute);
        }


        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (MismatchError(property) != null)
            {
                return true;
            }

            return !((AssetPreviewAttribute)saintsAttribute).Above;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return HelpBox.GetHeight(error, width, MessageType.Error);
            }

            return ((AssetPreviewAttribute)saintsAttribute).Above? 0: GetExtraHeight(property, width, saintsAttribute);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return HelpBox.Draw(position, error, MessageType.Error);
            }

            return Draw(position, property, saintsAttribute);
        }

        private float GetExtraHeight(SerializedProperty property, float width, ISaintsAttribute saintsAttribute)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            int maxWidth = assetPreviewAttribute.MaxWidth;
            // int useWidth = maxWidth == -1? Mathf.FloorToInt(width): Mathf.Min(maxWidth, Mathf.FloorToInt(width));
            int maxHeight = assetPreviewAttribute.MaxHeight;

            Texture2D previewTexture = GetPreview(maxWidth, maxHeight, width, property.objectReferenceValue);
            // ReSharper disable once Unity.NoNullPropagation
            return previewTexture?.height ?? width;
        }

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute)
        {

            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            int maxWidth = assetPreviewAttribute.MaxWidth;
            // int useWidth = maxWidth == -1? Mathf.FloorToInt(position.width): Mathf.Min(maxWidth, Mathf.FloorToInt(position.width));
            // int maxHeight = Mathf.Min(assetPreviewAttribute.MaxHeight, Mathf.FloorToInt(position.height));
            int maxHeight = assetPreviewAttribute.MaxHeight;

            Texture2D previewTexture = GetPreview(maxWidth, maxHeight, position.width, property.objectReferenceValue);

            if (previewTexture == null || previewTexture.width == 1)
            {
                return position;
            }

            EAlign align = assetPreviewAttribute.Align;
            float xOffset;
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (align)
            {
                case EAlign.Start:
                    xOffset = 0;
                    break;
                case EAlign.Center:
                    xOffset = (position.width - previewTexture.width) / 2;
                    break;
                case EAlign.End:
                    xOffset = position.width - previewTexture.width;
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
                height = previewTexture.height,
            };

            GUI.Label(previewRect, previewTexture);

            return RectUtils.SplitHeightRect(position, previewTexture.height).leftRect;
        }

        private static string MismatchError(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return $"Expect string or int, get {property.propertyType}";
            }
            return property.objectReferenceValue == null
                ? "field is null"
                : null;
        }
    }
}
