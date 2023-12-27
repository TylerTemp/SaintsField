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
        // private int _cachedWidth;
        // private int _cachedHeight;
        // private Texture2D _cachedWidthTexture;

        ~AssetPreviewAttributeDrawer()
        {
            if (_previewTexture)
            {
                Object.DestroyImmediate(_previewTexture);
            }

            // if (_cachedWidthTexture)
            // {
            //     Object.DestroyImmediate(_cachedWidthTexture);
            // }
        }

        private Texture2D GetPreview(float viewWidth, Object target)
        {
            if (viewWidth < 0)
            {
                return null;
            }

            // if (property.propertyType != SerializedPropertyType.ObjectReference ||
            //     property.objectReferenceValue == null)
            // {
            //     return null;
            // }
            // Debug.Log($"check preview {_previewTexture?.width}");
            if(_previewTexture == null || _previewTexture.width == 1)
            {
                // Debug.Log($"load preview {target}");
                if(AssetPreview.IsLoadingAssetPreview(target.GetInstanceID()))
                {
                    return null;
                }
                _previewTexture = AssetPreview.GetAssetPreview(target);
            }

            if (_previewTexture == null || _previewTexture.width == 1)
            {
                return null;
            }

            return _previewTexture;

            // bool widthOk = width == -1 || _previewTexture.width <= viewWidth;
            // bool heightOk = maxHeight == -1 && _previewTexture.height <= maxHeight;
            // if (widthOk && heightOk)
            // {
            //     return _cachedWidthTexture = _previewTexture;
            // }
            //
            // // Debug.Log($"viewWidth={viewWidth}, width={width}, maxHeight={maxHeight}, _previewTexture.width={_previewTexture.width}, _previewTexture.height={_previewTexture.height}");
            // (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), width, maxHeight, _previewTexture.width, _previewTexture.height);
            // // Debug.Log($"scaleWidth={scaleWidth}, scaleHeight={scaleHeight}");
            //
            // if (_cachedWidth == scaleWidth && _cachedHeight == scaleHeight && _cachedWidthTexture != null && _cachedWidthTexture.width != 1 && _cachedWidthTexture.height != 1)
            // {
            //     return _cachedWidthTexture;
            // }
            // _cachedWidth = scaleWidth;
            // _cachedHeight = scaleHeight;
            // // return _cachedWidthTexture = formatted;
            //
            // // _cachedWidthTexture = Tex.TextureTo(_previewTexture, scaleWidth, scaleHeight);
            // _cachedWidthTexture = _previewTexture;
            //
            // if (_cachedWidthTexture.width == 1)
            // {
            //     return _previewTexture;
            // }
            //
            // return _cachedWidthTexture;
        }

        private static (int width, int height) GetPreviewScaleSize(int width, int maxHeight, float viewWidth, Texture previewTexture)
        {
            return Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), width, maxHeight, previewTexture.width, previewTexture.height);
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

            Texture2D previewTexture = GetPreview(width, property.objectReferenceValue);
            if (previewTexture == null)
            {
                return 0;
            }

            var size = GetPreviewScaleSize(maxWidth, maxHeight, width, previewTexture);
            // ReSharper disable once Unity.NoNullPropagation
            float result = size.height;
            // Debug.Log($"GetExtraHeight: {result}");
            return result;
        }

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            int maxWidth = assetPreviewAttribute.MaxWidth;
            // int useWidth = maxWidth == -1? Mathf.FloorToInt(position.width): Mathf.Min(maxWidth, Mathf.FloorToInt(position.width));
            // int maxHeight = Mathf.Min(assetPreviewAttribute.MaxHeight, Mathf.FloorToInt(position.height));
            int maxHeight = assetPreviewAttribute.MaxHeight;

            // return position;

            Texture2D previewTexture = GetPreview(position.width, property.objectReferenceValue);

            if (previewTexture == null || previewTexture.width == 1)
            {
                return position;
            }

            // Debug.Log($"render texture {previewTexture.width}x{previewTexture.height} @ {position}");
            (int scaleWidth, int scaleHeight) = GetPreviewScaleSize(maxWidth, maxHeight, position.width, previewTexture);

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
