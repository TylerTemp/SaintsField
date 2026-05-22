#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.AssetPreviewDrawer
{
    public partial class AssetPreviewAttributeDrawer
    {
        public class AssetPreviewContainer : VisualElement
        {
            public readonly Image ImageElement;
            public readonly VisualElement InteractivePreviewContainer;

            public AssetPreviewContainer()
            {
                ImageElement = new Image
                {
                    scaleMode = ScaleMode.ScaleToFit,
                    sourceRect = new Rect(0, 0, 0, 0),
                    // style =
                    // {
                    //     flexGrow = 1,
                    //     // flexShrink = 1,
                    // },
                };
                hierarchy.Add(ImageElement);
                InteractivePreviewContainer = new VisualElement();
                hierarchy.Add(InteractivePreviewContainer);
            }
        }


        public class AssetPreviewField : BaseField<Object>
        {
            // public readonly VisualElement ImageContainerElement;
            // public readonly Image ImageElement;
            public readonly AssetPreviewContainer AssetPreviewContainer;

            public AssetPreviewField(string label, AssetPreviewContainer assetPreviewContainer) : base(label,
                assetPreviewContainer)
            {
                AssetPreviewContainer = assetPreviewContainer;
                // AssetPreviewContainer input = this.
                // ImageContainerElement = imageContainer;
                // ImageElement = image;
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

            // Texture2D preview = GetPreview(GetCurObject(property, info, parent));
            return CreateUIToolkitElement(property, index, assetPreviewAttribute);
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

            // Texture2D preview = GetPreview(GetCurObject(property, info, parent));
            return CreateUIToolkitElement(property, index, assetPreviewAttribute);
        }

        private class Payload
        {
            public Object ObjectReferenceValue;
            public float ProcessedWidth;
            public UnityEditor.Editor Editor;
            public bool UseEditor;
        }

        private const int InteractiveDefaultSize = 300;

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
            // Image image = root.ImageElement;
            Payload payload = (Payload)root.userData;

            Object curObject = GetCurObject(property, info, parent);

            if (ReferenceEquals(payload.ObjectReferenceValue, curObject))
            {
                // Debug.Log($"same object, skip {curObject}");
                return;
            }

            if (RuntimeUtil.IsNull(curObject))
            {
                payload.ObjectReferenceValue = null;
                root.style.display = DisplayStyle.None;
                // Debug.Log($"null object, skip");
                return;
            }

            payload.ObjectReferenceValue = curObject;

            bool hasPreviewGui;
            if (curObject is Texture or Sprite or Graphic or UnityEngine.UI.Image)
            {
                hasPreviewGui = false;
            }
            else
            {
                UnityEditor.Editor.CreateCachedEditor(
                    curObject,
                    null,
                    ref payload.Editor);
                hasPreviewGui = payload.Editor.HasPreviewGUI();
            }

            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;

            if (hasPreviewGui)
            {
                payload.UseEditor = true;
                UIToolkitUtils.SetDisplayStyle(root.AssetPreviewContainer.ImageElement, DisplayStyle.None);
                if(root.AssetPreviewContainer.InteractivePreviewContainer.childCount > 0)
                {
                    root.AssetPreviewContainer.InteractivePreviewContainer.Clear();
                }
                UIToolkitUtils.SetDisplayStyle(root.AssetPreviewContainer.InteractivePreviewContainer, DisplayStyle.Flex);

                root.AssetPreviewContainer.InteractivePreviewContainer.Add(new IMGUIContainer(() =>
                {
                    if (payload.Editor == null)
                    {
                        return;
                    }

                    float size = InteractiveDefaultSize;
                    if (payload.ProcessedWidth > 0)
                    {
                        size = payload.ProcessedWidth;
                    }
                    // Debug.Log(size);
                    Rect rect = GUILayoutUtility.GetRect(size, size);

                    // cachedEditor.OnPreviewGUI(
                    //     rect,
                    //     EditorStyles.helpBox);
                    payload.Editor.OnInteractivePreviewGUI(rect, EditorStyles.helpBox);
                }));
                OnRootGeoChanged(
                    root,
                    assetPreviewAttribute.Width, assetPreviewAttribute.Height);
            }
            else
            {
                // Debug.Log("processing preview thumbnail");
                if (payload.Editor != null)
                {
                    Object.DestroyImmediate(payload.Editor);
                    payload.Editor = null;
                }
                if(root.AssetPreviewContainer.InteractivePreviewContainer.childCount > 0)
                {

                    root.AssetPreviewContainer.InteractivePreviewContainer.Clear();
                }
                UIToolkitUtils.SetDisplayStyle(root.AssetPreviewContainer.InteractivePreviewContainer, DisplayStyle.None);
                payload.UseEditor = false;

                _previewTexture = null;
                Texture2D preview = GetPreview(curObject);
                // Debug.Log($"try get preview {preview!=null}");
                if (preview != null)
                {
                    UIToolkitUtils.SetDisplayStyle(root.AssetPreviewContainer.ImageElement, DisplayStyle.Flex);

                    SetImage(root, root.AssetPreviewContainer.ImageElement, preview);
                    // Debug.Log($"set preview {preview}");
                    OnRootGeoChanged(
                        root,
                        assetPreviewAttribute.Width, assetPreviewAttribute.Height);
                }
                else if (AssetPreview.IsLoadingAssetPreview(curObject.
#if UNITY_6000_4_OR_NEWER
                        GetEntityId
#else
                        GetInstanceID
#endif
                                ()))
                {
                    // Debug.Log("still processing, will be updated later");
                    payload.ObjectReferenceValue = null;
                }

            }
        }

        private static VisualElement CreateUIToolkitElement(SerializedProperty property, int index,
            AssetPreviewAttribute assetPreviewAttribute)
        {
            AssetPreviewContainer previewContainer = new AssetPreviewContainer
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            //
            // Image image = new Image
            // {
            //     scaleMode = ScaleMode.ScaleToFit,
            //     sourceRect = new Rect(0, 0, 0, 0),
            //     // style =
            //     // {
            //     //     flexGrow = 1,
            //     //     // flexShrink = 1,
            //     // },
            // };
            // VisualElement imageContainer = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = FlexDirection.Row,
            //     },
            // };
            // imageContainer.Add(image);

            AssetPreviewField assetPreviewField = new AssetPreviewField(" ", previewContainer)
            {
                name = NameRoot(property, index),
                userData = new Payload(),
            };

            assetPreviewField.AddToClassList(AssetPreviewField.alignedFieldUssClassName);

            switch (assetPreviewAttribute.Align)
            {
                case EAlign.Start:
                    assetPreviewField.labelElement.style.display = DisplayStyle.None;
                    break;
                case EAlign.Center:
                    assetPreviewField.labelElement.style.display = DisplayStyle.None;
                    previewContainer.style.justifyContent = Justify.Center;
                    break;
                case EAlign.End:
                    assetPreviewField.labelElement.style.display = DisplayStyle.None;
                    previewContainer.style.justifyContent = Justify.FlexEnd;
                    break;
                case EAlign.FieldStart:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetPreviewAttribute.Align),
                        assetPreviewAttribute.Align, null);
            }

            // UpdateImage(image, preview, assetPreviewAttribute);

            // image.AddToClassList(NameImage(property, index));

            // SetImage(assetPreviewField, image, preview);
            // if (preview == null)
            // {
            //     image.userData = new Payload
            //     {
            //         ObjectReferenceValue = null,
            //         ProcessedWidth = 0,
            //     };
            // }
            // else
            // {
            //     image.style.width = preview.width;
            //     image.style.height = preview.height;
            //     image.userData = new Payload
            //     {
            //         ObjectReferenceValue = GetCurObject(property, info, parent),
            //         ProcessedWidth = 0,
            //     };
            // }

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
            AssetPreviewContainer previewContainer = root.AssetPreviewContainer;
            if (root.style.display == DisplayStyle.None)
            {
                return;
            }

            float rootWidth = root.resolvedStyle.width;

            if (rootWidth < Mathf.Epsilon || float.IsNaN(rootWidth))
            {
                return;
            }

            float fakeLabelWidth;
            if (root.labelElement.style.display == DisplayStyle.None)
            {
                fakeLabelWidth = 0;
            }
            else
            {
                fakeLabelWidth = root.labelElement.resolvedStyle.width;
                if (float.IsNaN(fakeLabelWidth))
                {
                    return;
                }
            }

            Payload payload = (Payload)root.userData;
            if (Mathf.Approximately(payload.ProcessedWidth, rootWidth))
            {
                return;
            }

            if (payload.UseEditor)
            {
                int maxWidth = Mathf.FloorToInt(rootWidth - fakeLabelWidth);
                int width = InteractiveDefaultSize;
                // int height = InteractiveDefaultSize;
                if (maxWidth > 0)
                {
                    (width, _) = Tex.GetProperScaleRect(
                        maxWidth,
                        widthConfig, heightConfig, InteractiveDefaultSize, InteractiveDefaultSize);
                }
                payload.ProcessedWidth = width;
            }
            else
            {
                payload.ProcessedWidth = rootWidth;

                if (previewContainer.ImageElement.image == null)
                {
                    return;
                }
                int maxWidth = Mathf.FloorToInt(rootWidth - fakeLabelWidth);

                int width = 0;
                int height = 0;
                if (maxWidth > 0)
                {
                    Texture2D preview = (Texture2D)previewContainer.ImageElement.image;
                    (width, height) = Tex.GetProperScaleRect(
                        maxWidth,
                        widthConfig, heightConfig, preview.width, preview.height);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ASSET_PREVIEW
                    Debug.Log($"{width}x{height}<-rootWidth={rootWidth}, fakeLabel={fakeLabelWidth}, widthConfig={widthConfig}, heightConfig={heightConfig}, preview={preview.width}x{preview.height}");
#endif
                }

                previewContainer.ImageElement.style.width = Mathf.Max(width, 0);
                previewContainer.ImageElement.style.height = Mathf.Max(height, 0);
            }

        }
    }
}
#endif
