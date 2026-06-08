using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.AssetPreviewDrawer
{
    public partial class AssetPreviewAttributeDrawer
    {
        private class ImGuiCacheInfo
        {
            public Object ObjectReferenceValue;
            public UnityEditor.Editor Editor;
            public bool UseEditor;
        }

        private static readonly Dictionary<string, ImGuiCacheInfo> ImGuiCache = new Dictionary<string, ImGuiCacheInfo>();

        private static ImGuiCacheInfo EnsureKey(SerializedProperty property, int propertyIndex)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{propertyIndex}";
            if (ImGuiCache.TryGetValue(key, out ImGuiCacheInfo info))
            {
                return info;
            }

            info = new ImGuiCacheInfo();
            ImGuiCache[key] = info;
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                if (info.Editor != null)
                {
                    Object.DestroyImmediate(info.Editor);
                    info.Editor = null;
                }
                ImGuiCache.Remove(key);
            });

            return info;
        }

        private static void PrepareCache(ImGuiCacheInfo cache, Object curObject)
        {
            if (ReferenceEquals(cache.ObjectReferenceValue, curObject))
            {
                return;
            }

            cache.ObjectReferenceValue = curObject;
            cache.UseEditor = false;

            if (cache.Editor != null)
            {
                Object.DestroyImmediate(cache.Editor);
                cache.Editor = null;
            }

            if (RuntimeUtil.IsNull(curObject) || curObject is Texture or Sprite or UnityEngine.UI.Graphic or UnityEngine.UI.Image)
            {
                return;
            }

            UnityEditor.Editor.CreateCachedEditor(curObject, null, ref cache.Editor);
            if (curObject is Component comp && cache.Editor != null && !cache.Editor.HasPreviewGUI())
            {
                Object.DestroyImmediate(cache.Editor);
                cache.Editor = null;
                UnityEditor.Editor.CreateCachedEditor(comp.gameObject, null, ref cache.Editor);
            }
            cache.UseEditor = cache.Editor != null && cache.Editor.HasPreviewGUI();
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return ((AssetPreviewAttribute)saintsAttribute).Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = MismatchError(property, info, parent);
            if (error != null)
            {
                return 0;
            }

            return ((AssetPreviewAttribute)saintsAttribute).Above
                ? GetExtraHeight(property, width, saintsAttribute, index, info, parent)
                : 0;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            string error = MismatchError(property, info, parent);
            if (error != null)
            {
                return position;
            }

            return Draw(position, property, saintsAttribute, index: 0, info, parent);
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            if (MismatchError(property, info, parent) != null)
            {
                return true;
            }

            return !((AssetPreviewAttribute)saintsAttribute).Above;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = MismatchError(property, info, parent);
            if (error != null)
            {
                return ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            return ((AssetPreviewAttribute)saintsAttribute).Above
                ? 0
                : GetExtraHeight(property, width, saintsAttribute, index, info, parent);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = MismatchError(property, info, parent);
            if (error != null)
            {
                return ImGuiHelpBox.Draw(position, error, MessageType.Error);
            }

            return Draw(position, property, saintsAttribute, index, info, parent);
        }

        private float GetExtraHeight(SerializedProperty property, float width, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            if (width - 1 < Mathf.Epsilon)
            {
                return 0;
            }

            ImGuiCacheInfo cache = EnsureKey(property, index);
            Object curObject = GetCurObject(property, info, parent);
            PrepareCache(cache, curObject);
            if (cache.UseEditor)
            {
                float useWidthForEditor = assetPreviewAttribute.Align == EAlign.FieldStart
                    ? Mathf.Max(width - EditorGUIUtility.labelWidth, 1)
                    : width;
                int maxWidth = Mathf.FloorToInt(useWidthForEditor);
                int resultWidth = InteractiveDefaultSize;
                if (maxWidth > 0)
                {
                    (resultWidth, _) = Tex.GetProperScaleRect(
                        maxWidth,
                        assetPreviewAttribute.Width, assetPreviewAttribute.Height,
                        InteractiveDefaultSize, InteractiveDefaultSize);
                }
                return resultWidth;
            }

            Texture2D previewTexture = GetPreview(curObject);
            if (previewTexture == null)
            {
                return 0;
            }

            float useWidth = assetPreviewAttribute.Align == EAlign.FieldStart
                ? Mathf.Max(width - EditorGUIUtility.labelWidth, 1)
                : width;

            (int _, int height) size = Tex.GetProperScaleRect(
                Mathf.FloorToInt(useWidth),
                assetPreviewAttribute.Width, assetPreviewAttribute.Height,
                previewTexture.width, previewTexture.height);
            return size.height;
        }

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            if (position.width - 1 < Mathf.Epsilon)
            {
                return position;
            }

            ImGuiCacheInfo cache = EnsureKey(property, index);
            Object curObject = GetCurObject(property, info, parent);
            PrepareCache(cache, curObject);

            float useWidth = assetPreviewAttribute.Align == EAlign.FieldStart
                ? Mathf.Max(position.width - EditorGUIUtility.labelWidth, 1)
                : position.width;

            int maxWidth = Mathf.FloorToInt(useWidth);

            if (cache.UseEditor && cache.Editor != null)
            {
                int scaleSize = InteractiveDefaultSize;
                if (maxWidth > 0)
                {
                    (scaleSize, _) = Tex.GetProperScaleRect(
                        maxWidth,
                        assetPreviewAttribute.Width, assetPreviewAttribute.Height,
                        InteractiveDefaultSize, InteractiveDefaultSize);
                }

                float xOffsetInteractive;
                switch (assetPreviewAttribute.Align)
                {
                    case EAlign.Start:
                        xOffsetInteractive = 0;
                        break;
                    case EAlign.Center:
                        xOffsetInteractive = (position.width - scaleSize) / 2f;
                        break;
                    case EAlign.End:
                        xOffsetInteractive = position.width - scaleSize;
                        break;
                    case EAlign.FieldStart:
                        xOffsetInteractive = EditorGUIUtility.labelWidth;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Rect previewRect = new Rect(position.x + xOffsetInteractive, position.y, scaleSize, scaleSize);
                cache.Editor.OnInteractivePreviewGUI(previewRect, EditorStyles.helpBox);
                return RectUtils.SplitHeightRect(position, scaleSize).leftRect;
            }

            Texture2D previewTexture = GetPreview(curObject);
            if (previewTexture == null || previewTexture.width == 1)
            {
                return position;
            }

            (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(
                maxWidth,
                assetPreviewAttribute.Width, assetPreviewAttribute.Height,
                previewTexture.width, previewTexture.height);

            float xOffset;
            switch (assetPreviewAttribute.Align)
            {
                case EAlign.Start:
                    xOffset = 0;
                    break;
                case EAlign.Center:
                    xOffset = (position.width - scaleWidth) / 2f;
                    break;
                case EAlign.End:
                    xOffset = position.width - scaleWidth;
                    break;
                case EAlign.FieldStart:
                    xOffset = EditorGUIUtility.labelWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Rect textureRect = new Rect(position)
            {
                x = position.x + xOffset,
                width = scaleWidth,
                height = scaleHeight,
            };
            GUI.DrawTexture(textureRect, previewTexture, ScaleMode.ScaleToFit);
            return RectUtils.SplitHeightRect(position, scaleHeight).leftRect;
        }
    }
}
