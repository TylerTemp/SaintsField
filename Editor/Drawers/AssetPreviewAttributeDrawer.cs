using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
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

        private Texture2D GetPreview(Object target)
        {
            if (!target)
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

        #region IMGUI

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute)
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

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return position;
            }

            return Draw(position, property, saintsAttribute);
        }


        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
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
                return ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            return ((AssetPreviewAttribute)saintsAttribute).Above? 0: GetExtraHeight(property, width, saintsAttribute);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return ImGuiHelpBox.Draw(position, error, MessageType.Error);
            }

            return Draw(position, property, saintsAttribute);
        }

        private float GetExtraHeight(SerializedProperty property, float width, ISaintsAttribute saintsAttribute)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            int maxWidth = assetPreviewAttribute.MaxWidth;
            // int useWidth = maxWidth == -1? Mathf.FloorToInt(width): Mathf.Min(maxWidth, Mathf.FloorToInt(width));
            int maxHeight = assetPreviewAttribute.MaxHeight;

            if (width < 0)
            {
                return 0;
            }

            Texture2D previewTexture = GetPreview(property.objectReferenceValue);
            if (previewTexture == null)
            {
                return 0;
            }

            (int width, int height) size = GetPreviewScaleSize(maxWidth, maxHeight, width, previewTexture);
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

            if (position.width < 0)
            {
                return position;
            }

            Texture2D previewTexture = GetPreview(property.objectReferenceValue);

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
        #endregion

        private static string ClassImage(SerializedProperty property) => $"{property.propertyPath}__Image";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            if (!assetPreviewAttribute.Above)
            {
                return null;
            }

            Texture2D preview = GetPreview(property.objectReferenceValue);
            Image image = CreateImage(property, preview, assetPreviewAttribute);
            return image;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            if (assetPreviewAttribute.Above)
            {
                return null;
            }

            Texture2D preview = GetPreview(property.objectReferenceValue);
            Image image = CreateImage(property, preview, assetPreviewAttribute);
            return image;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, object parent)
        {
            Image image = container.Query<Image>(className: ClassImage(property)).First();
            int preInstanceId = (int)image.userData;
            int curInstanceId = property.objectReferenceValue?.GetInstanceID() ?? 0;

            // Debug.Log($"cur={curInstanceId}, pre={preInstanceId}");
            if (image.image != null && preInstanceId == curInstanceId)
            {
                return;
            }

            _previewTexture = null;
            image.style.display = DisplayStyle.None;

            Texture2D preview = GetPreview(property.objectReferenceValue);

            UpdateImage(image, preview, (AssetPreviewAttribute)saintsAttribute);
            if (preview != null)
            {
                image.userData = curInstanceId;
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static Image CreateImage(SerializedProperty property, Texture2D preview, AssetPreviewAttribute assetPreviewAttribute)
        {
            Image image = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                userData = property.objectReferenceValue?.GetInstanceID() ?? int.MinValue ,
            };

            switch (assetPreviewAttribute.Align)
            {
                case EAlign.Start:
                    // image.style.alignSelf = Align.Start;
                    break;
                case EAlign.Center:
                    image.style.alignSelf = Align.Center;
                    break;
                case EAlign.End:
                    image.style.alignSelf = Align.FlexEnd;
                    break;
                case EAlign.FieldStart:
                    // image.style.alignSelf = Align.FlexStart;
                    image.style.left = LabelBaseWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetPreviewAttribute.Align), assetPreviewAttribute.Align, null);
            }

            UpdateImage(image, preview, assetPreviewAttribute);

            image.AddToClassList(ClassImage(property));
            return image;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void UpdateImage(Image image, Texture2D preview, AssetPreviewAttribute assetPreviewAttribute)
        {
            image.image = preview;
            if (preview == null)
            {
                image.style.display = DisplayStyle.None;
                return;
            }

            image.style.display = DisplayStyle.Flex;

            if (assetPreviewAttribute.MaxWidth > 0 || assetPreviewAttribute.MaxHeight > 0)
            {
                int imageWidth = preview.width;
                int imageHeight = preview.height;
                int maxHeight = assetPreviewAttribute.MaxHeight > 0? assetPreviewAttribute.MaxHeight: imageWidth;
                int maxWidth = assetPreviewAttribute.MaxWidth > 0? assetPreviewAttribute.MaxWidth: imageHeight;

                (int fittedWidth, int fittedHeight) = Tex.FitScale(maxWidth, maxHeight, imageWidth, imageHeight);

                image.style.maxWidth = fittedWidth;
                image.style.maxHeight = fittedHeight;
            }
            else
            {
                image.style.maxWidth = image.style.maxHeight = StyleKeyword.Null;
            }

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
