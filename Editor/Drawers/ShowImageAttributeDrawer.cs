using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UIElements.Image;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ShowImageAttribute))]
    [CustomPropertyDrawer(typeof(AboveImageAttribute))]
    [CustomPropertyDrawer(typeof(BelowImageAttribute))]
    public class ShowImageAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private Texture2D _originTexture;
        // private Texture2D _previewTexture;

        private string _error = "";

        // ~ShowImageAttributeDrawer()
        // {
        //     CleanPreviewTexture(_previewTexture);
        //     _previewTexture = null;
        // }

        // private static void CleanPreviewTexture(Texture2D texture2D)
        // {
        //     if(texture2D && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture2D)))
        //     {
        //         Object.DestroyImmediate(texture2D);
        //         // _previewTexture = null;
        //     }
        // }

        private (Texture2D, float preferredWidth, float preferredHeight) GetPreview(SerializedProperty property, int maxWidth, int maxHeight, float viewWidth, string name, FieldInfo info, object parent)
        {
            // Debug.Log($"viewWidth={viewWidth}");
            if (viewWidth - 1f < Mathf.Epsilon)
            {
                return (null, maxWidth, maxWidth);
            }

            (string error, Texture2D image) = GetImage(property, name, info, parent);
            _error = error;

            if(_error != "")
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

            if (widthOk && heightOk)  // original width & smaller than view width
            {
                return (_originTexture, _originTexture.width, _originTexture.height);
            }

            // fixed width / overflow height
            (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), maxWidth, maxHeight, _originTexture.width, _originTexture.height);
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

            return ((ShowImageAttribute)saintsAttribute).Above? GetImageHeight(property, width, saintsAttribute, info, parent): 0;
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
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
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            // Debug.Log($"draw below view width: {width}");

            if (_error != "")
            {
                return ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
            }

            float height = ((ShowImageAttribute)saintsAttribute).Above? 0: GetImageHeight(property, width, saintsAttribute, info, parent);
            // Debug.Log($"GetBlowExtraHeight={height}");
            return height;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            if (_error != "")
            {
                return ImGuiHelpBox.Draw(position, _error, MessageType.Error);
            }

            // EditorGUI.DrawRect(position, Color.blue);
            // Debug.Log($"DrawBelow height: {position.height}");

            return Draw(position, property, saintsAttribute, info, parent);
        }

        private float GetImageHeight(SerializedProperty property, float width, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            int maxWidth = showImageAttribute.MaxWidth;
            // int viewWidth = Mathf.FloorToInt(width);
            // int useWidth = maxWidth == -1? viewWidth: Mathf.Min(maxWidth, viewWidth);
            int maxHeight = showImageAttribute.MaxHeight;

            (Texture2D _, float _, float preferredHeight) = GetPreview(property, maxWidth, maxHeight, width, showImageAttribute.ImageCallback, info, parent);

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

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
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

            (Texture2D previewTexture, float preferredWidth, float preferredHeight)  = GetPreview(property, maxWidth, maxHeight, position.width, showImageAttribute.ImageCallback, info, parent);

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
        #endregion

        private static (string error, Texture2D image) GetImage(SerializedProperty property, string name, FieldInfo info, object target)
        {
            if (string.IsNullOrEmpty(name))
            {
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (property.propertyType == SerializedPropertyType.Generic)
                {
                    (string _, IWrapProp getResult) = Util.GetOf<IWrapProp>(property.name, null, property, info, target);
                    // Debug.Log(getResult);
                    if (getResult != null)
                    {
                        object actualValue = Util.GetWrapValue(getResult);
                        // Debug.Log(actualValue);
                        return GetImageFromTarget(actualValue);
                    }
                    return ($"property {property.propertyPath} is not supported.", null);
                }

                if(property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    return ($"Expect ObjectReference for `{property.propertyPath}`, get {property.propertyType}", null);
                }

                return GetImageFromTarget(GetCurObject(property, info, target));
            }

            // search parent first
            (string reflectError, object fieldValue) = Util.GetOf<object>(name, null, property, info, target);
            if(reflectError == "")
            {
                if (fieldValue is IWrapProp wrapProp)
                {
                    fieldValue = Util.GetWrapValue(wrapProp);
                }

                Texture2D reflect2D;
                (reflectError, reflect2D) = GetImageFromTarget(fieldValue);
                if (reflectError == "")
                {
                    return (reflectError, reflect2D);
                }
            }

            // SerializedProperty prop = property.serializedObject.FindProperty(name) ?? SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, name);
            // if (prop != null)
            // {
            //     // ReSharper disable once ConvertIfStatementToReturnStatement
            //     if (prop.propertyType == SerializedPropertyType.Generic)
            //     {
            //         GetCurObject(prop, info, parent);
            //     }
            //     else if(prop.propertyType != SerializedPropertyType.ObjectReference)
            //     {
            //         return ($"Expect ObjectReference for `{name}`, get {prop.propertyType}", null);
            //     }
            //
            //     return GetImageFromTarget(prop.objectReferenceValue);
            // }

            return ($"not found `{name}` on `{target}`", null);
        }

        private static UnityEngine.Object GetCurObject(SerializedProperty property, FieldInfo info, object parent)
        {
            // Object curObject = null;
            if (property.propertyType != SerializedPropertyType.Generic)
            {
                return property.objectReferenceValue;
            }

            (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

            if (error == "" && propertyValue is IWrapProp wrapProp)
            {
                return Util.GetWrapValue(wrapProp) as UnityEngine.Object;
            }

            return null;
        }

        private static (string error, Texture2D image) GetImageFromTarget(object result)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (result)
            {
                case Sprite sprite:
                    return ("", sprite.texture);
                case Texture2D texture2D:
                    return ("", texture2D);
                // case Texture texture:
                //     targetChanged = !ReferenceEquals(_originTexture, texture);
                //     _originTexture = texture as Texture2D;
                //     break;
                case SpriteRenderer spriteRenderer:
                    return ("", spriteRenderer.sprite == null? null: spriteRenderer.sprite.texture);
                case UnityEngine.UI.Image image:
                    return ("", image.sprite == null? null: image.sprite.texture);
                case RawImage image:
                    return ("", image.texture as Texture2D);
                case UnityEngine.UI.Button button:
                    // targetChanged = !ReferenceEquals(_originTexture, button.targetGraphic.mainTexture);
                    return button.targetGraphic?
                        ("", button.targetGraphic.mainTexture as Texture2D):
                        ("", null);
                case GameObject _:
                case Component _:
                {
                    UnityEngine.Object obj = (UnityEngine.Object)result;
                    UnityEngine.Object actualObj = Util.GetTypeFromObj(obj, typeof(SpriteRenderer))
                                                   ?? Util.GetTypeFromObj(obj, typeof(UnityEngine.UI.Image))
                                                   ?? Util.GetTypeFromObj(obj, typeof(RawImage))
                                                   ?? Util.GetTypeFromObj(obj, typeof(UnityEngine.UI.Button))
                                                   ;
                    // Debug.Log($"obj={obj} actual={actualObj}, renderer={((Component)foundObj).GetComponent<Renderer>()}");
                    // ReSharper disable once TailRecursiveCall
                    return GetImageFromTarget(
                        actualObj
                    );
                }
                default:
                    return (
                        $"Unable to find image on {(result == null ? "null" : result.GetType().ToString())}",
                        null);
            }
        }

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit

        public class ShowImageField : BaseField<string>
        {
            public readonly VisualElement imageContainerElement;
            public readonly Image imageElement;

            public ShowImageField(string label, VisualElement imageContainer, Image image) : base(label, imageContainer)
            {
                imageContainerElement = imageContainer;
                imageElement = image;
            }
        }

        private static string NameRoot(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ShowImage";

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
                helpBox.style.display = error == ""? DisplayStyle.None: DisplayStyle.Flex;
            }

            ShowImageField root = container.Q<ShowImageField>(NameRoot(property, index));
            Image image = root.imageElement;
            Payload payload = (Payload)image.userData;
            // ReSharper disable once Unity.NoNullPropagation
            // int curInstanceId = property.objectReferenceValue?.GetInstanceID() ?? 0;

            // Debug.Log($"cur={curInstanceId}, pre={preInstanceId}");
            if (ReferenceEquals(payload.Image, preview))
            {
                return;
            }

            root.style.display = DisplayStyle.None;

            if(preview == null)
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

        // ReSharper disable once SuggestBaseTypeForParameter
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
                    showImageField.imageContainerElement.style.justifyContent = Justify.Center;
                    break;
                case EAlign.End:
                    showImageField.labelElement.style.display = DisplayStyle.None;
                    showImageField.imageContainerElement.style.justifyContent = Justify.FlexEnd;
                    break;
                case EAlign.FieldStart:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(showImageAttribute.Align), showImageAttribute.Align, null);
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

            showImageField.RegisterCallback<GeometryChangedEvent>(_ => OnRootGeoChanged(showImageField, showImageAttribute.MaxWidth, showImageAttribute.MaxHeight));

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
            root.imageElement.image = preview;
        }

        private static void OnRootGeoChanged(ShowImageField root, int widthConfig, int heightConfig)
        {
            Label fakeLabel = root.labelElement;
            Image image = root.imageElement;

            if(root.style.display == DisplayStyle.None)
            {
                return;
            }

            float rootWidth = root.resolvedStyle.width;

            if(rootWidth < Mathf.Epsilon)
            {
                return;
            }

            float fakeLabelWidth = fakeLabel.style.display == DisplayStyle.None
                ? 0
                : fakeLabel.resolvedStyle.width;

            float imageWidth = image.resolvedStyle.width;

            if(new []{rootWidth, fakeLabelWidth, imageWidth}.Any(float.IsNaN))
            {
                return;
            }

            Payload payload = (Payload)image.userData;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(payload.ProcessedWidth == rootWidth)
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
        #endregion
#endif
    }
}
