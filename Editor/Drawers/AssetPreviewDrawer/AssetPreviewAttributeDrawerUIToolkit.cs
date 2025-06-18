#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.AssetPreviewDrawer
{
    public partial class AssetPreviewAttributeDrawer
    {

        public class AssetPreviewField : BaseField<string>
        {
            public readonly VisualElement ImageContainerElement;
            public readonly Image ImageElement;

            public AssetPreviewField(string label, VisualElement imageContainer, Image image) : base(label,
                imageContainer)
            {
                ImageContainerElement = imageContainer;
                ImageElement = image;
            }
        }

        private static string NameRoot(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__AssetPreview";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            if (!assetPreviewAttribute.Above)
            {
                return null;
            }

            Texture2D preview = GetPreview(GetCurObject(property, info, parent));
            return CreateUIToolkitElement(property, index, preview, assetPreviewAttribute, info, parent);
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            if (assetPreviewAttribute.Above)
            {
                return null;
            }

            Texture2D preview = GetPreview(GetCurObject(property, info, parent));
            return CreateUIToolkitElement(property, index, preview, assetPreviewAttribute, info, parent);
        }

        private struct Payload
        {
            // ReSharper disable InconsistentNaming
            public Object ObjectReferenceValue;

            public float ProcessedWidth;
            // ReSharper enable InconsistentNaming
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                return;
            }

            AssetPreviewField root = container.Q<AssetPreviewField>(NameRoot(property, index));
            Image image = root.ImageElement;
            Payload payload = (Payload)image.userData;

            Object curObject = GetCurObject(property, info, parent);

            // Debug.Log($"cur={curInstanceId}, pre={preInstanceId}");
            if (ReferenceEquals(payload.ObjectReferenceValue, curObject))
            {
                return;
            }

            _previewTexture = null;
            root.style.display = DisplayStyle.None;

            if (curObject == null)
            {
                image.userData = new Payload
                {
                    ObjectReferenceValue = null,
                    ProcessedWidth = payload.ProcessedWidth,
                };
                return;
            }

            Texture2D preview = GetPreview(curObject);

            // UpdateImage(image, preview, (AssetPreviewAttribute)saintsAttribute);
            if (preview != null)
            {
                AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
                SetImage(root, image, preview);
                OnRootGeoChanged(
                    root,
                    assetPreviewAttribute.Width, assetPreviewAttribute.Height);
            }
        }

        private static VisualElement CreateUIToolkitElement(SerializedProperty property, int index, Texture2D preview,
            AssetPreviewAttribute assetPreviewAttribute, FieldInfo info, object parent)
        {
            Image image = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                sourceRect = new Rect(0, 0, 0, 0),
                // style =
                // {
                //     flexGrow = 1,
                //     // flexShrink = 1,
                // },
            };
            VisualElement imageContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            imageContainer.Add(image);

            AssetPreviewField assetPreviewField = new AssetPreviewField(" ", imageContainer, image)
            {
                name = NameRoot(property, index),
            };

            assetPreviewField.AddToClassList("unity-base-field__aligned");

            switch (assetPreviewAttribute.Align)
            {
                case EAlign.Start:
                    assetPreviewField.labelElement.style.display = DisplayStyle.None;
                    break;
                case EAlign.Center:
                    assetPreviewField.labelElement.style.display = DisplayStyle.None;
                    assetPreviewField.ImageContainerElement.style.justifyContent = Justify.Center;
                    break;
                case EAlign.End:
                    assetPreviewField.labelElement.style.display = DisplayStyle.None;
                    assetPreviewField.ImageContainerElement.style.justifyContent = Justify.FlexEnd;
                    break;
                case EAlign.FieldStart:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetPreviewAttribute.Align),
                        assetPreviewAttribute.Align, null);
            }

            // UpdateImage(image, preview, assetPreviewAttribute);

            // image.AddToClassList(NameImage(property, index));

            SetImage(assetPreviewField, image, preview);
            if (preview == null)
            {
                image.userData = new Payload
                {
                    ObjectReferenceValue = null,
                    ProcessedWidth = 0,
                };
            }
            else
            {
                image.style.width = preview.width;
                image.style.height = preview.height;
                image.userData = new Payload
                {
                    ObjectReferenceValue = GetCurObject(property, info, parent),
                    ProcessedWidth = 0,
                };
            }

            assetPreviewField.AddToClassList(ClassAllowDisable);

            assetPreviewField.RegisterCallback<GeometryChangedEvent>(_ =>
                OnRootGeoChanged(assetPreviewField, assetPreviewAttribute.Width, assetPreviewAttribute.Height));

            return assetPreviewField;
        }

        private static void SetImage(VisualElement root, Image image, Texture2D preview)
        {
            if (preview == null)
            {
                root.style.display = DisplayStyle.None;
                return;
            }

            root.style.display = DisplayStyle.Flex;
            image.image = preview;
        }

        private static void OnRootGeoChanged(AssetPreviewField root, int widthConfig, int heightConfig)
        {
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

            float fakeLabelWidth = root.labelElement.style.display == DisplayStyle.None
                ? 0
                : root.labelElement.resolvedStyle.width;

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
                ObjectReferenceValue = payload.ObjectReferenceValue,
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ASSET_PREVIEW
                Debug.Log($"{width}x{height}<-rootWidth={rootWidth}, fakeLabel={fakeLabelWidth}, widthConfig={widthConfig}, heightConfig={heightConfig}, preview={preview.width}x{preview.height}");
#endif
            }

            image.style.width = Mathf.Max(width, 0);
            image.style.height = Mathf.Max(height, 0);
        }
    }
}
#endif
