using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ShowImageDrawer
{
    public partial class ShowImageAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public object ImageSource;
            public Texture2D Image;
            public Rect TexCoords;
            public float SourceWidth;
            public float SourceHeight;
            public float PreferredWidth;
            public float PreferredHeight;
            public float LastViewWidth = -1f;
            public int LastUpdateFrame = -1;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property, int index)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}[{index}]";
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return infoCache;
        }

        private void RefreshCache(InfoIMGUI cache, SerializedProperty property, ShowImageAttribute showImageAttribute,
            float viewWidth, FieldInfo info, object parent)
        {
            float normalizedWidth = Mathf.Max(viewWidth, 0f);
            if (cache.LastUpdateFrame == Time.frameCount && Mathf.Abs(cache.LastViewWidth - normalizedWidth) < 0.5f)
            {
                return;
            }

            cache.LastUpdateFrame = Time.frameCount;
            cache.LastViewWidth = normalizedWidth;
            cache.Error = "";
            cache.ImageSource = null;
            cache.Image = null;
            cache.TexCoords = new Rect(0f, 0f, 1f, 1f);
            cache.SourceWidth = 0f;
            cache.SourceHeight = 0f;
            cache.PreferredWidth = 0f;
            cache.PreferredHeight = 0f;

            if (normalizedWidth < Mathf.Epsilon)
            {
                return;
            }

            (string error, object imageSource) = GetImageSource(property, showImageAttribute.ImageCallback, info, parent);
            cache.Error = error;
            cache.ImageSource = imageSource;
            cache.Image = error == "" && imageSource != null ? GetImageTextureFromSource(imageSource) : null;
            cache.TexCoords = GetTexCoords(imageSource);
            (cache.SourceWidth, cache.SourceHeight) = GetSourceSize(imageSource, cache.Image);

            if (error != "" || cache.Image == null)
            {
                return;
            }

            float availableWidth = normalizedWidth;
            if (showImageAttribute.Align == EAlign.FieldStart)
            {
                availableWidth -= EditorGUIUtility.labelWidth;
            }

            availableWidth = Mathf.Max(availableWidth, 0f);
            if (availableWidth < Mathf.Epsilon)
            {
                return;
            }

            bool widthOk = showImageAttribute.MaxWidth == -1
                ? cache.SourceWidth <= availableWidth
                : cache.SourceWidth <= showImageAttribute.MaxWidth;
            bool heightOk = showImageAttribute.MaxHeight == -1 || cache.SourceHeight <= showImageAttribute.MaxHeight;

            if (widthOk && heightOk)
            {
                cache.PreferredWidth = cache.SourceWidth;
                cache.PreferredHeight = cache.SourceHeight;
                return;
            }

            (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(
                Mathf.FloorToInt(availableWidth),
                showImageAttribute.MaxWidth,
                showImageAttribute.MaxHeight,
                Mathf.RoundToInt(cache.SourceWidth),
                Mathf.RoundToInt(cache.SourceHeight));
            cache.PreferredWidth = scaleWidth;
            cache.PreferredHeight = scaleHeight;
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            return ((ShowImageAttribute)saintsAttribute).Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            InfoIMGUI cache = EnsureKey(property, index);
            RefreshCache(cache, property, showImageAttribute, width, info, parent);

            if (cache.Error != "")
            {
                return 0;
            }

            return showImageAttribute.Above ? cache.PreferredHeight : 0;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            InfoIMGUI cache = EnsureKey(property, index);
            RefreshCache(cache, property, showImageAttribute, position.width, info, parent);

            if (cache.Error != "")
            {
                return position;
            }

            return Draw(position, showImageAttribute, cache);
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            InfoIMGUI cache = EnsureKey(property, index);
            RefreshCache(cache, property, showImageAttribute, EditorGUIUtility.currentViewWidth, info, parent);
            return !showImageAttribute.Above || cache.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            InfoIMGUI cache = EnsureKey(property, index);
            RefreshCache(cache, property, showImageAttribute, width, info, parent);

            if (cache.Error != "")
            {
                return ImGuiHelpBox.GetHeight(cache.Error, width, MessageType.Error);
            }

            return showImageAttribute.Above ? 0 : cache.PreferredHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            ShowImageAttribute showImageAttribute = (ShowImageAttribute)saintsAttribute;
            InfoIMGUI cache = EnsureKey(property, index);
            RefreshCache(cache, property, showImageAttribute, position.width, info, parent);

            if (cache.Error != "")
            {
                return ImGuiHelpBox.Draw(position, cache.Error, MessageType.Error);
            }

            return Draw(position, showImageAttribute, cache);
        }

        private static Rect Draw(Rect position, ShowImageAttribute showImageAttribute, InfoIMGUI cache)
        {
            if (position.height <= 0 || cache.Image == null || cache.PreferredHeight <= 0 || cache.PreferredWidth <= 0)
            {
                return position;
            }

            float xOffset;
            switch (showImageAttribute.Align)
            {
                case EAlign.Start:
                    xOffset = 0;
                    break;
                case EAlign.Center:
                    xOffset = (position.width - cache.PreferredWidth) / 2f;
                    break;
                case EAlign.End:
                    xOffset = position.width - cache.PreferredWidth;
                    break;
                case EAlign.FieldStart:
                    xOffset = EditorGUIUtility.labelWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(showImageAttribute.Align), showImageAttribute.Align, null);
            }

            Rect previewRect = new Rect(position)
            {
                x = position.x + xOffset,
                width = cache.PreferredWidth,
                height = cache.PreferredHeight,
            };

            GUI.DrawTextureWithTexCoords(previewRect, cache.Image, cache.TexCoords);
            return RectUtils.SplitHeightRect(position, cache.PreferredHeight).leftRect;
        }

        private static Rect GetTexCoords(object imageSource)
        {
            if (imageSource is not Sprite sprite || sprite.texture == null)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            Rect textureRect = sprite.textureRect;
            Texture2D texture = sprite.texture;
            return new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);
        }

        private static (float width, float height) GetSourceSize(object imageSource, Texture2D texture)
        {
            if (imageSource is Sprite sprite)
            {
                Rect textureRect = sprite.textureRect;
                return (textureRect.width, textureRect.height);
            }

            return texture == null ? (0f, 0f) : (texture.width, texture.height);
        }

        private static Texture2D GetImageTextureFromSource(object imageSource)
        {
            switch (imageSource)
            {
                case Sprite sprite:
                    return sprite.texture;
                case Texture2D texture2D:
                    return texture2D;
                default:
                    return null;
            }
        }
    }
}
