using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

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

        private (Texture2D, float preferredWidth, float preferredHeight) GetPreview(SerializedProperty property, int maxWidth, int maxHeight, float viewWidth, string name, object parent)
        {
            // Debug.Log($"viewWidth={viewWidth}");
            if (viewWidth - 1f < Mathf.Epsilon)
            {
                return (null, maxWidth, maxWidth);
            }

            (string error, Texture2D image) = GetImage(property, name, parent);
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
            object parent)
        {
            return ((ShowImageAttribute)saintsAttribute).Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            if (_error != "")
            {
                return 0;
            }

            return ((ShowImageAttribute)saintsAttribute).Above? GetImageHeight(property, width, saintsAttribute, parent): 0;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            if (_error != "")
            {
                return position;
            }

            return Draw(position, property, saintsAttribute, parent);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            // Debug.Log($"{((ShowImageAttribute)saintsAttribute).Above}/{_error}");
            bool willDrawBelow = !((ShowImageAttribute)saintsAttribute).Above || _error != "";
            // Debug.Log($"WillDrawBelow={willDrawBelow}");
            return willDrawBelow;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            // Debug.Log($"draw below view width: {width}");

            if (_error != "")
            {
                return ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
            }

            float height = ((ShowImageAttribute)saintsAttribute).Above? 0: GetImageHeight(property, width, saintsAttribute, parent);
            // Debug.Log($"GetBlowExtraHeight={height}");
            return height;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            if (_error != "")
            {
                return ImGuiHelpBox.Draw(position, _error, MessageType.Error);
            }

            // EditorGUI.DrawRect(position, Color.blue);
            // Debug.Log($"DrawBelow height: {position.height}");

            return Draw(position, property, saintsAttribute, parent);
        }

        private float GetImageHeight(SerializedProperty property, float width, ISaintsAttribute saintsAttribute, object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            int maxWidth = showImageAttribute.MaxWidth;
            // int viewWidth = Mathf.FloorToInt(width);
            // int useWidth = maxWidth == -1? viewWidth: Mathf.Min(maxWidth, viewWidth);
            int maxHeight = showImageAttribute.MaxHeight;

            (Texture2D _, float _, float preferredHeight) = GetPreview(property, maxWidth, maxHeight, width, showImageAttribute.ImageCallback, parent);

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

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute, object parent)
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

            (Texture2D previewTexture, float preferredWidth, float preferredHeight)  = GetPreview(property, maxWidth, maxHeight, position.width, showImageAttribute.ImageCallback, parent);

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

        private static (string error, Texture2D image) GetImage(SerializedProperty property, string name, object target)
        {

            if (string.IsNullOrEmpty(name))
            {
                if(property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    return ($"Expect ObjectReference for `{name}`, get {property.propertyType}", null);
                }

                return GetImageFromTarget(property.objectReferenceValue);
            }

            SerializedProperty prop = property.serializedObject.FindProperty(name) ?? SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, name);
            if (prop != null)
            {
                if(prop.propertyType != SerializedPropertyType.ObjectReference)
                {
                    return ($"Expect ObjectReference for `{name}`, get {prop.propertyType}", null);
                }

                return GetImageFromTarget(prop.objectReferenceValue);

            }

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(target.GetType(), name);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.Field:
                {
                    // targetChanged = CheckSetOriginalTextureAndError(((FieldInfo)fieldOrMethodInfo).GetValue(target));
                    return GetImageFromTarget(((FieldInfo)fieldOrMethodInfo).GetValue(target));
                }

                case ReflectUtils.GetPropType.Property:
                {
                    // targetChanged = CheckSetOriginalTextureAndError(((PropertyInfo)fieldOrMethodInfo).GetValue(target));
                    return GetImageFromTarget(((PropertyInfo)fieldOrMethodInfo).GetValue(target));
                }
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    Object result;
                    try
                    {
                        result = (Object)methodInfo.Invoke(target,
                            methodParams.Select(p => p.DefaultValue).ToArray());
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.LogException(e);
                        Debug.Assert(e.InnerException != null);
                        return (e.InnerException.Message, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        return (e.Message, null);
                    }

                    // targetChanged = CheckSetOriginalTextureAndError(result);
                    return GetImageFromTarget(result);
                }
                case ReflectUtils.GetPropType.NotFound:
                {
                    return ($"not found `{name}` on `{target}`", null);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }
        }

        private static (string error, Texture2D image) GetImageFromTarget(object result)
        {
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
                    return ("", spriteRenderer.sprite.texture);
                case Image image:
                    return ("", image.sprite.texture);
                case RawImage image:
                    return ("", image.texture as Texture2D);
                case Button button:
                    // targetChanged = !ReferenceEquals(_originTexture, button.targetGraphic.mainTexture);
                    return button.targetGraphic?
                        ("", button.targetGraphic.mainTexture as Texture2D):
                        ("", null);
                default:
                    return (
                        $"Unable to find image on {(result == null ? "null" : result.GetType().ToString())}",
                        null);
            }
        }

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit

        private static string NameImage(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ShowImage";

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__ShowImage_HelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            if (!showImageAttribute.Above)
            {
                return null;
            }

            return CreateImage(property, index, null, showImageAttribute);
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
                name = NameImage(property, index) + "_root",
            };
            root.Add(CreateImage(property, index, null, showImageAttribute));
            root.Add(helpBox);

            return root;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;

            (string error, Texture2D preview) = GetImage(property, showImageAttribute.ImageCallback, parent);

            UnityEngine.UIElements.Image image = container.Q<UnityEngine.UIElements.Image>(NameImage(property, index));
            UpdateImage(image, preview, showImageAttribute);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            // ReSharper disable once InvertIf
            if (helpBox.text != error)
            {
                helpBox.text = error;
                helpBox.style.display = error == ""? DisplayStyle.None: DisplayStyle.Flex;
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static UnityEngine.UIElements.Image CreateImage(SerializedProperty property, int index, Texture2D preview,
            ShowImageAttribute showImageAttribute)
        {
            UnityEngine.UIElements.Image image = new UnityEngine.UIElements.Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                name = NameImage(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            switch (showImageAttribute.Align)
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
                    throw new ArgumentOutOfRangeException(nameof(showImageAttribute.Align), showImageAttribute.Align, null);
            }

            UpdateImage(image, preview, showImageAttribute);

            return image;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void UpdateImage(UnityEngine.UIElements.Image image, Texture2D preview, ShowImageAttribute showImageAttribute)
        {
            if (ReferenceEquals(image.image, preview))
            {
                return;
            }

            image.image = preview;

            bool isNull = preview == null;

            image.style.display = isNull? DisplayStyle.None: DisplayStyle.Flex;
            if (isNull)
            {
                return;
            }

            if (showImageAttribute.MaxWidth > 0 || showImageAttribute.MaxHeight > 0)
            {
                int imageWidth = preview.width;
                int imageHeight = preview.height;
                int maxHeight = showImageAttribute.MaxHeight > 0? showImageAttribute.MaxHeight: imageWidth;
                int maxWidth = showImageAttribute.MaxWidth > 0? showImageAttribute.MaxWidth: imageHeight;

                (int fittedWidth, int fittedHeight) = Tex.FitScale(maxWidth, maxHeight, imageWidth, imageHeight);

                image.style.maxWidth = fittedWidth;
                image.style.maxHeight = fittedHeight;
            }
            else
            {
                image.style.maxWidth = image.style.maxHeight = StyleKeyword.Null;
            }

        }
        #endregion
#endif
    }
}
