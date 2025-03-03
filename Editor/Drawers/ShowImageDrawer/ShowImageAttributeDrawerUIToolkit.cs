#if UNITY_2021_3_OR_NEWER
using System;
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
        public class ShowImageField : BaseField<string>
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

            return CreateUIToolkitElement(property, index, null, showImageAttribute);
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
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
            root.Add(CreateUIToolkitElement(property, index, null, showImageAttribute));
            root.Add(helpBox);

            root.AddToClassList(ClassAllowDisable);

            return root;
        }

        private struct Payload
        {
            // ReSharper disable InconsistentNaming
            public Texture2D Image;

            public float ProcessedWidth;
            // ReSharper enable InconsistentNaming
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            (string error, Texture2D preview) = GetImage(property, showImageAttribute.ImageCallback, info, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            // ReSharper disable once InvertIf
            if (helpBox.text != error)
            {
                helpBox.text = error;
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }

            ShowImageField root = container.Q<ShowImageField>(NameRoot(property, index));
            Image image = root.ImageElement;
            Payload payload = (Payload)image.userData;
            // ReSharper disable once Unity.NoNullPropagation
            // int curInstanceId = property.objectReferenceValue?.GetInstanceID() ?? 0;

            // Debug.Log($"cur={curInstanceId}, pre={preInstanceId}");
            if (ReferenceEquals(payload.Image, preview))
            {
                return;
            }

            root.style.display = DisplayStyle.None;

            if (preview == null)
            {
                image.userData = new Payload
                {
                    Image = null,
                    ProcessedWidth = payload.ProcessedWidth,
                };
                return;
            }

            // UpdateImage(image, preview, (AssetPreviewAttribute)saintsAttribute);
            if (preview != null)
            {
                SetImage(root, preview);
                OnRootGeoChanged(
                    root,
                    showImageAttribute.MaxWidth, showImageAttribute.MaxHeight);
            }
        }

        private static VisualElement CreateUIToolkitElement(SerializedProperty property, int index, Texture2D preview,
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
            showImageField.AddToClassList("unity-base-field__aligned");


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

            // UpdateImage(image, preview, assetPreviewAttribute);

            // image.AddToClassList(NameImage(property, index));

            SetImage(showImageField, preview);
            if (preview == null)
            {
                image.userData = new Payload
                {
                    Image = null,
                    ProcessedWidth = 0,
                };
            }
            else
            {
                image.style.width = preview.width;
                image.style.height = preview.height;
                image.userData = new Payload
                {
                    Image = preview,
                    ProcessedWidth = 0,
                };
            }

            showImageField.AddToClassList(ClassAllowDisable);

            showImageField.RegisterCallback<GeometryChangedEvent>(_ =>
                OnRootGeoChanged(showImageField, showImageAttribute.MaxWidth, showImageAttribute.MaxHeight));

            return showImageField;
        }

        private static void SetImage(ShowImageField root, Texture2D preview)
        {
            if (preview == null)
            {
                root.style.display = DisplayStyle.None;
                return;
            }

            root.style.display = DisplayStyle.Flex;
            root.ImageElement.image = preview;
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

            Payload payload = (Payload)image.userData;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (payload.ProcessedWidth == rootWidth)
            {
                return;
            }

            image.userData = new Payload
            {
                Image = payload.Image,
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
    }
}
#endif
