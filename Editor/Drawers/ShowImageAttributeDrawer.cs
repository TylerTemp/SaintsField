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
        private Texture2D _previewTexture;

        private string _error = "";

        ~ShowImageAttributeDrawer()
        {
            CleanPreviewTexture(_previewTexture);
            _previewTexture = null;
        }

        private static void CleanPreviewTexture(Texture2D texture2D)
        {
            if(texture2D && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture2D)))
            {
                Object.DestroyImmediate(texture2D);
                // _previewTexture = null;
            }
        }

        private Texture2D GetPreview(SerializedProperty property, int width, int maxHeight, float viewWidth, string name)
        {
            _error = "";

            bool targetChanged;
            SerializedProperty prop = property.serializedObject.FindProperty(name) ?? SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, name);
            if (prop != null)
            {
                if(prop.propertyType != SerializedPropertyType.ObjectReference)
                {
                    _error = $"Expect ObjectReference for `{name}`, get {prop.propertyType}";
                    return null;
                }

                targetChanged = CheckSetOriginalTextureAndError(prop.objectReferenceValue);

                if(_error != "")
                {
                    return null;
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
                        targetChanged = CheckSetOriginalTextureAndError(((FieldInfo)fieldOrMethodInfo).GetValue(target));
                        break;
                    }

                    case ReflectUtils.GetPropType.Property:
                    {
                        targetChanged = CheckSetOriginalTextureAndError(((PropertyInfo)fieldOrMethodInfo).GetValue(target));
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
                            _error = e.InnerException!.Message;
                            Debug.LogException(e);
                            return null;
                        }
                        catch (Exception e)
                        {
                            _error = e.Message;
                            return null;
                        }

                        targetChanged = CheckSetOriginalTextureAndError(result);

                        break;
                    }
                    case ReflectUtils.GetPropType.NotFound:
                    {
                        _error =
                            $"not found `{name}` on `{target}`";
                        return null;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }
            }

            if(_error != "")
            {
                return null;
            }

            bool previewTextureNull = _previewTexture == null;
            if(!previewTextureNull && _previewTexture.width == 1 && _previewTexture.height == 1)
            {
                targetChanged = true;
            }

            if (!previewTextureNull && !targetChanged)
            {
                return _previewTexture;
            }

            if (_originTexture == null)
            {
                return null;
            }

            CleanPreviewTexture(_previewTexture);

            bool widthOk = width == -1 || _originTexture.width <= viewWidth;
            bool heightOk = maxHeight == -1 || _originTexture.height <= maxHeight;

            if (widthOk && heightOk)  // original width & smaller than view width
            {
                _previewTexture = _originTexture;
            }
            else  // fixed width / overflow height
            {
                (int scaleWidth, int scaleHeight) = SaintsField.Utils.Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), width, maxHeight, _originTexture.width, _originTexture.height);
                _previewTexture = SaintsField.Utils.Tex.TextureTo(_originTexture, scaleWidth, scaleHeight);
            }

            if (_originTexture.width <= width && (maxHeight == -1 || _previewTexture.height <= maxHeight))
            {
                // _previewTexture = SaintsField.Utils.Tex.ConvertToCompatibleFormat(_originTexture);
                // Debug.Log($"use original height {_originTexture.height}");
                _previewTexture = _originTexture;
            }
            else
            {
                // Debug.Log($"use original height {_originTexture.height}");
                _previewTexture = SaintsField.Utils.Tex.TextureTo(_originTexture, width, maxHeight);
            }

            return _previewTexture;
        }

        private bool CheckSetOriginalTextureAndError(object result)
        {
            bool targetChanged;

            _error = "";
            switch (result)
            {
                case Sprite sprite:
                    targetChanged = !ReferenceEquals(_originTexture, sprite.texture);
                    _originTexture = sprite.texture;
                    break;
                case Texture2D texture2D:
                    targetChanged = !ReferenceEquals(_originTexture, texture2D);
                    _originTexture = texture2D;
                    break;
                // case Texture texture:
                //     targetChanged = !ReferenceEquals(_originTexture, texture);
                //     _originTexture = texture as Texture2D;
                //     break;
                case SpriteRenderer spriteRenderer:
                    targetChanged = !ReferenceEquals(_originTexture, spriteRenderer.sprite.texture);
                    _originTexture = spriteRenderer.sprite.texture;
                    break;
                case Image image:
                    targetChanged = !ReferenceEquals(_originTexture, image.sprite.texture);
                    _originTexture = image.sprite.texture;
                    break;
                case RawImage image:
                    targetChanged = !ReferenceEquals(_originTexture, image.texture);
                    _originTexture = image.texture as Texture2D;
                    break;
                case Button button:
                    targetChanged = !ReferenceEquals(_originTexture, button.targetGraphic.mainTexture);
                    _originTexture = button.targetGraphic.mainTexture as Texture2D;
                    break;
                default:
                    _error = $"Expect Sprite or Texture2D, get {(result == null? "null": result.GetType().ToString())}";
                    return false;
            }

            return targetChanged;
        }

        protected override bool WillDrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
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

        protected override Rect DrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (_error != "")
            {
                return position;
            }

            return Draw(position, property, saintsAttribute);
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // Debug.Log($"{((ShowImageAttribute)saintsAttribute).Above}/{_error}");
            bool willDrawBelow = !((ShowImageAttribute)saintsAttribute).Above || _error != "";
            // Debug.Log($"WillDrawBelow={willDrawBelow}");
            return willDrawBelow;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute)
        {
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

            Texture2D previewTexture = GetPreview(property, maxWidth, maxHeight, width, showImageAttribute.ImageCallback);

            // if (previewTexture)
            // {
            //     Debug.Log($"get preview {previewTexture.width}x{previewTexture.height}");
            // }
            // else
            // {
            //     Debug.Log($"get no preview");
            // }
            // ReSharper disable once Unity.NoNullPropagation
            return previewTexture?.height ?? 0;
        }

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            int maxWidth = showImageAttribute.MaxWidth;
            // int useWidth = maxWidth == -1? Mathf.FloorToInt(position.width): Mathf.Min(maxWidth, Mathf.FloorToInt(position.width));
            // int maxHeight = Mathf.Min(assetPreviewAttribute.MaxHeight, Mathf.FloorToInt(position.height));
            int maxHeight = showImageAttribute.MaxHeight;

            Texture2D previewTexture = GetPreview(property, maxWidth, maxHeight, position.width, showImageAttribute.ImageCallback);

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
    }
}
