using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ShowImageAttribute))]
    [CustomPropertyDrawer(typeof(AboveImageAttribute))]
    [CustomPropertyDrawer(typeof(BelowImageAttribute))]
    public class ShowImageAttributeDrawer: SaintsPropertyDrawer
    {
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

        private (Texture2D, float preferredWidth, float preferredHeight) GetPreview(SerializedProperty property, int maxWidth, int maxHeight, float viewWidth, string name)
        {
            // Debug.Log($"viewWidth={viewWidth}");
            if (viewWidth - 1f < Mathf.Epsilon)
            {
                return (null, maxWidth, maxWidth);
            }

            _error = "";

            if (string.IsNullOrEmpty(name))
            {
                CheckSetOriginalTextureAndError(property.objectReferenceValue);
            }
            else
            {
                SerializedProperty prop = property.serializedObject.FindProperty(name) ?? SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, name);
                if (prop != null)
                {
                    if(prop.propertyType != SerializedPropertyType.ObjectReference)
                    {
                        _error = $"Expect ObjectReference for `{name}`, get {prop.propertyType}";
                        return (null, 0, 0);
                    }

                    // targetChanged = CheckSetOriginalTextureAndError(prop.objectReferenceValue);
                    CheckSetOriginalTextureAndError(prop.objectReferenceValue);

                    if(_error != "")
                    {
                        return (null, 0, 0);
                    }
                }
                else
                {
                    _error = "";
                    object target = GetParentTarget(property);
                    (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                        ReflectUtils.GetProp(target.GetType(), name);
                    switch (getPropType)
                    {
                        case ReflectUtils.GetPropType.Field:
                        {
                            // targetChanged = CheckSetOriginalTextureAndError(((FieldInfo)fieldOrMethodInfo).GetValue(target));
                            CheckSetOriginalTextureAndError(((FieldInfo)fieldOrMethodInfo).GetValue(target));
                            break;
                        }

                        case ReflectUtils.GetPropType.Property:
                        {
                            // targetChanged = CheckSetOriginalTextureAndError(((PropertyInfo)fieldOrMethodInfo).GetValue(target));
                            CheckSetOriginalTextureAndError(((PropertyInfo)fieldOrMethodInfo).GetValue(target));
                            break;
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
                                Debug.Assert(e.InnerException != null);
                                _error = e.InnerException.Message;
                                Debug.LogException(e);
                                return (null, 0, 0);
                            }
                            catch (Exception e)
                            {
                                _error = e.Message;
                                return (null, 0, 0);
                            }

                            // targetChanged = CheckSetOriginalTextureAndError(result);
                            CheckSetOriginalTextureAndError(result);

                            break;
                        }
                        case ReflectUtils.GetPropType.NotFound:
                        {
                            _error =
                                $"not found `{name}` on `{target}`";
                            return (null, 0, 0);
                        }
                        default:
                            throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                    }
                }
            }

            if(_error != "")
            {
                return (null, 0, 0);
            }

            // bool previewTextureNull = _previewTexture == null;
            // if(!previewTextureNull && _previewTexture.width == 1 && _previewTexture.height == 1)
            // {
            //     targetChanged = true;
            // }
            //
            // if (!previewTextureNull && !targetChanged)
            // {
            //     return _previewTexture;
            // }

            if (_originTexture == null)
            {
                return (null, 0, 0);
            }

            // CleanPreviewTexture(_previewTexture);

            bool widthOk = maxWidth == -1
                ? _originTexture.width <= viewWidth
                : _originTexture.width <= maxWidth;
            bool heightOk = maxHeight == -1 || _originTexture.height <= maxHeight;

            if (widthOk && heightOk)  // original width & smaller than view width
            {
                // Debug.Log($"_originTexture {_originTexture.width}x{_originTexture.height} <= viewWidth {viewWidth}; width={width}");
                return (_originTexture, _originTexture.width, _originTexture.height);
            }

            // fixed width / overflow height
            (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), maxWidth, maxHeight, _originTexture.width, _originTexture.height);
            // Debug.Log($"scale to {scaleWidth}x{scaleHeight}; from {_originTexture.width}x{_originTexture.height}; viewWidth={viewWidth}, fitTo={maxWidth}x{maxHeight}");
            return (_originTexture, scaleWidth, scaleHeight);
            // _previewTexture = SaintsField.Utils.Tex.TextureTo(_originTexture, scaleWidth, scaleHeight);

            // if (_originTexture.width <= width && (maxHeight == -1 || _previewTexture.height <= maxHeight))
            // {
            //     // _previewTexture = SaintsField.Utils.Tex.ConvertToCompatibleFormat(_originTexture);
            //     // Debug.Log($"use original height {_originTexture.height}");
            //     _previewTexture = _originTexture;
            // }
            // else
            // {
            //     // Debug.Log($"use original height {_originTexture.height}");
            //     _previewTexture = SaintsField.Utils.Tex.TextureTo(_originTexture, width, maxHeight);
            // }
            //
            // return _previewTexture;
        }

        private void CheckSetOriginalTextureAndError(object result)
        {
            // bool targetChanged;

            _error = "";
            switch (result)
            {
                case Sprite sprite:
                    // targetChanged = !ReferenceEquals(_originTexture, sprite.texture);
                    _originTexture = sprite.texture;
                    break;
                case Texture2D texture2D:
                    // targetChanged = !ReferenceEquals(_originTexture, texture2D);
                    _originTexture = texture2D;
                    break;
                // case Texture texture:
                //     targetChanged = !ReferenceEquals(_originTexture, texture);
                //     _originTexture = texture as Texture2D;
                //     break;
                case SpriteRenderer spriteRenderer:
                    // targetChanged = !ReferenceEquals(_originTexture, spriteRenderer.sprite.texture);
                    _originTexture = spriteRenderer.sprite.texture;
                    break;
                case Image image:
                    // targetChanged = !ReferenceEquals(_originTexture, image.sprite.texture);
                    _originTexture = image.sprite.texture;
                    break;
                case RawImage image:
                    // targetChanged = !ReferenceEquals(_originTexture, image.texture);
                    _originTexture = image.texture as Texture2D;
                    break;
                case Button button:
                    // targetChanged = !ReferenceEquals(_originTexture, button.targetGraphic.mainTexture);
                    _originTexture = button.targetGraphic.mainTexture as Texture2D;
                    break;
                default:
                    _error = $"Expect Sprite or Texture2D, get {(result == null? "null": result.GetType().ToString())}";
                    // return false;
                    break;
            }

            // return targetChanged;
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return ((ShowImageAttribute)saintsAttribute).Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute)
        {
            if (_error != "")
            {
                return 0;
            }

            return ((ShowImageAttribute)saintsAttribute).Above? GetImageHeight(property, width, saintsAttribute): 0;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (_error != "")
            {
                return position;
            }

            return Draw(position, property, saintsAttribute);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            // Debug.Log($"{((ShowImageAttribute)saintsAttribute).Above}/{_error}");
            bool willDrawBelow = !((ShowImageAttribute)saintsAttribute).Above || _error != "";
            // Debug.Log($"WillDrawBelow={willDrawBelow}");
            return willDrawBelow;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute)
        {
            // Debug.Log($"draw below view width: {width}");

            if (_error != "")
            {
                return HelpBox.GetHeight(_error, width, MessageType.Error);
            }

            float height = ((ShowImageAttribute)saintsAttribute).Above? 0: GetImageHeight(property, width, saintsAttribute);
            // Debug.Log($"GetBlowExtraHeight={height}");
            return height;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (_error != "")
            {
                return HelpBox.Draw(position, _error, MessageType.Error);
            }

            // EditorGUI.DrawRect(position, Color.blue);
            // Debug.Log($"DrawBelow height: {position.height}");

            return Draw(position, property, saintsAttribute);
        }

        private float GetImageHeight(SerializedProperty property, float width, ISaintsAttribute saintsAttribute)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            int maxWidth = showImageAttribute.MaxWidth;
            // int viewWidth = Mathf.FloorToInt(width);
            // int useWidth = maxWidth == -1? viewWidth: Mathf.Min(maxWidth, viewWidth);
            int maxHeight = showImageAttribute.MaxHeight;

            (Texture2D _, float _, float preferredHeight) = GetPreview(property, maxWidth, maxHeight, width, showImageAttribute.ImageCallback);

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

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute)
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

            (Texture2D previewTexture, float preferredWidth, float preferredHeight)  = GetPreview(property, maxWidth, maxHeight, position.width, showImageAttribute.ImageCallback);

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
    }
}
