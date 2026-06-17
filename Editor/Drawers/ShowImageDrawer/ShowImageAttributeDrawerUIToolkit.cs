#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ShowImageDrawer
{
    public partial class ShowImageAttributeDrawer
    {
        private readonly Dictionary<Sprite, Texture2D> _cachedSpriteToTexture2D = new Dictionary<Sprite, Texture2D>();

        private sealed class ShowImageField : BaseField<string>
        {
            public readonly VisualElement ImageContainerElement;
            public readonly Image ImageElement;

            public ShowImageField(string label, VisualElement imageContainer, Image image) : base(label, imageContainer)
            {
                ImageContainerElement = imageContainer;
                ImageElement = image;
            }
        }

        private static string NameRoot(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__ShowImage";

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__ShowImage_HelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            if (!showImageAttribute.Above)
            {
                return null;
            }

            return CreateUIToolkitElement(property, index, showImageAttribute);
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            if (showImageAttribute.Above)
            {
                return helpBox;
            }

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            root.Add(CreateUIToolkitElement(property, index, showImageAttribute));
            root.Add(helpBox);

            root.AddToClassList(ClassAllowDisable);

            return root;
        }

        private struct ImageState
        {
            public Texture2D Preview;
            public float ProcessedWidth;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            (string error, object imageSource) = GetImageSource(property, showImageAttribute.ImageCallback, info, parent);
            Texture2D preview = error == "" ? GetPreviewTextureForUIToolkit(imageSource) : null;

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            if (helpBox.text != error)
            {
                helpBox.text = error;
                DisplayStyle helpDisplay = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                if (helpBox.style.display != helpDisplay)
                {
                    helpBox.style.display = helpDisplay;
                }
            }

            ShowImageField root = container.Q<ShowImageField>(NameRoot(property, index));
            Image image = root.ImageElement;

            DisplayStyle imageDisplay = error == "" ? DisplayStyle.Flex : DisplayStyle.None;
            if (preview == null)
            {
                imageDisplay = DisplayStyle.None;
            }

            if (image.style.display != imageDisplay)
            {
                image.style.display = imageDisplay;
            }

            ImageState imageState = (ImageState)image.userData;
            if (ReferenceEquals(imageState.Preview, preview))
            {
                return;
            }

            root.style.display = DisplayStyle.None;

            if (preview == null)
            {
                image.userData = new ImageState
                {
                    Preview = null,
                    ProcessedWidth = imageState.ProcessedWidth,
                };
                return;
            }

            SetImage(root, preview);
            image.userData = new ImageState
            {
                Preview = preview,
                ProcessedWidth = 0,
            };
            OnRootGeoChanged(root, showImageAttribute.MaxWidth, showImageAttribute.MaxHeight);
        }

        private static VisualElement CreateUIToolkitElement(SerializedProperty property, int index,
            ShowImageAttribute showImageAttribute)
        {
            Image image = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
            };

            VisualElement imageContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            imageContainer.Add(image);

            ShowImageField showImageField = new ShowImageField(" ", imageContainer, image)
            {
                name = NameRoot(property, index),
                style =
                {
                    flexGrow = 1,
                },
            };
            showImageField.AddToClassList(ShowImageField.alignedFieldUssClassName);

            switch (showImageAttribute.Align)
            {
                case EAlign.Start:
                    showImageField.labelElement.style.display = DisplayStyle.None;
                    break;
                case EAlign.Center:
                    showImageField.labelElement.style.display = DisplayStyle.None;
                    showImageField.ImageContainerElement.style.justifyContent = Justify.Center;
                    break;
                case EAlign.End:
                    showImageField.labelElement.style.display = DisplayStyle.None;
                    showImageField.ImageContainerElement.style.justifyContent = Justify.FlexEnd;
                    break;
                case EAlign.FieldStart:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(showImageAttribute.Align), showImageAttribute.Align,
                        null);
            }

            SetImage(showImageField, null);
            image.userData = new ImageState
            {
                Preview = null,
                ProcessedWidth = 0,
            };

            showImageField.AddToClassList(ClassAllowDisable);

            showImageField.RegisterCallback<GeometryChangedEvent>(_ =>
                OnRootGeoChanged(showImageField, showImageAttribute.MaxWidth, showImageAttribute.MaxHeight));

            return showImageField;
        }

        private static void SetImage(ShowImageField root, Texture2D preview)
        {
            root.ImageElement.image = preview;
            if (preview == null)
            {
                root.style.display = DisplayStyle.None;
                return;
            }

            root.style.display = DisplayStyle.Flex;
        }

        private static void OnRootGeoChanged(ShowImageField root, int widthConfig, int heightConfig)
        {
            Label fakeLabel = root.labelElement;
            Image image = root.ImageElement;

            if (root.style.display == DisplayStyle.None)
            {
                return;
            }

            float rootWidth = root.resolvedStyle.width;

            if (rootWidth < Mathf.Epsilon)
            {
                return;
            }

            float fakeLabelWidth = fakeLabel.style.display == DisplayStyle.None
                ? 0
                : fakeLabel.resolvedStyle.width;

            float imageWidth = image.resolvedStyle.width;

            if (new[] { rootWidth, fakeLabelWidth, imageWidth }.Any(float.IsNaN))
            {
                return;
            }

            ImageState imageState = (ImageState)image.userData;

            if (Mathf.Abs(imageState.ProcessedWidth - rootWidth) < 0.5f)
            {
                return;
            }

            image.userData = new ImageState
            {
                Preview = imageState.Preview,
                ProcessedWidth = rootWidth,
            };

            if (image.image == null)
            {
                return;
            }

            int maxWidth = Mathf.FloorToInt(rootWidth - fakeLabelWidth);
            int width = 0;
            int height = 0;
            if (maxWidth > 0)
            {
                Texture2D preview = (Texture2D)image.image;
                (width, height) = Tex.GetProperScaleRect(
                    maxWidth,
                    widthConfig, heightConfig, preview.width, preview.height);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_ASSET_PREVIEW
                Debug.Log($"{width}x{height}<-rootWidth={rootWidth}, fakeLabel={fakeLabelWidth}, widthConfig={widthConfig}, heightConfig={heightConfig}, preview={preview.width}x{preview.height}");
#endif
            }

            root.schedule.Execute(() =>
            {
                image.style.width = Mathf.Max(width, 0);
                image.style.height = Mathf.Max(height, 0);
            });
        }

        private Texture2D GetPreviewTextureForUIToolkit(object imageSource)
        {
            switch (imageSource)
            {
                case Sprite sprite:
                    if (_cachedSpriteToTexture2D.TryGetValue(sprite, out Texture2D cachedImage) && cachedImage != null)
                    {
                        return cachedImage;
                    }

                    if (IsPartOfMultipleSprite(sprite))
                    {
                        return _cachedSpriteToTexture2D[sprite] = GetPartOfTheSprite(sprite);
                    }

                    return _cachedSpriteToTexture2D[sprite] = sprite.texture;
                case Texture2D texture2D:
                    return texture2D;
                default:
                    return null;
            }
        }

        private static bool IsPartOfMultipleSprite(Sprite sprite)
        {
            string path = AssetDatabase.GetAssetPath(sprite.texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            return importer != null && importer.spriteImportMode == SpriteImportMode.Multiple;
        }

        private static Texture2D GetPartOfTheSprite(Sprite sprite)
        {
            Texture2D originalTexture = sprite.texture;
            Texture2D readWriteTexture = Tex.ConvertToCompatibleFormat(originalTexture);
            Rect textureRect = sprite.textureRect;

            Texture2D croppedTexture = new Texture2D((int)textureRect.width, (int)textureRect.height);
            Color[] pixels = readWriteTexture.GetPixels(
                (int)textureRect.x,
                (int)textureRect.y,
                (int)textureRect.width,
                (int)textureRect.height);

            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            return croppedTexture;
        }
    }
}
#endif
