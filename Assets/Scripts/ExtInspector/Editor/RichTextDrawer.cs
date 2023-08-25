using System;
using System.Collections.Generic;
using ExtInspector.Editor.Utils;
using ExtInspector.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ExtInspector.Editor
{
    public class RichTextDrawer: IDisposable
    {
        // cache
        private struct TextureCacheKey
        {
            public EColor EColor;
            public string IconResourcePath;
            public bool IsEditorResource;

            public override bool Equals(object obj)
            {
                if (obj is not TextureCacheKey other)
                {
                    return false;
                }

                return EColor == other.EColor && IconResourcePath == other.IconResourcePath && IsEditorResource == other.IsEditorResource;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (int)EColor;
                    hashCode = (hashCode * 397) ^ (IconResourcePath != null ? IconResourcePath.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ IsEditorResource.GetHashCode();
                    return hashCode;
                }
            }
        }

        private readonly Dictionary<TextureCacheKey, Texture> _textureCache = new Dictionary<TextureCacheKey, Texture>();

        public void Dispose()
        {
            foreach (Texture cacheValue in _textureCache.Values)
            {
                UnityEngine.Object.Destroy(cacheValue);
            }
            _textureCache.Clear();
        }

        public void DrawLabel(Rect position, GUIContent oldLabel, IEnumerable<RichText.RichTextPayload> payloads)
        {
            Rect accPosition = position;
            foreach (RichText.RichTextPayload richTextPayload in payloads)
            {
                // Rect curRect;
                // Rect leftRect;
                GUIStyle labelStyle = EditorStyles.label;
                GUIContent label = oldLabel;

                switch (richTextPayload)
                {
                    case RichText.ColoredLabelPayload coloredLabelPayload:
                        labelStyle = new GUIStyle(GUI.skin.label)
                        {
                            normal =
                            {
                                textColor = coloredLabelPayload.Color.GetColor(),
                            },
                        };
                        break;
                    case RichText.LabelPayload _:
                        break;
                    case RichText.ColoredTextPayload coloredTextPayload:
                        labelStyle = new GUIStyle(GUI.skin.label)
                        {
                            normal =
                            {
                                textColor = coloredTextPayload.Color.GetColor(),
                            },
                        };
                        label = new GUIContent(coloredTextPayload.Text);
                        break;
                    case RichText.TextPayload textPayload:
                        label = new GUIContent(textPayload.Text);
                        break;
                    case RichText.ColoredIconPayload coloredIconPayload:
                    {
                        TextureCacheKey cacheKey = new TextureCacheKey
                        {
                            EColor = coloredIconPayload.Color,
                            IconResourcePath = coloredIconPayload.IconResourcePath,
                            IsEditorResource = coloredIconPayload.IsEditorResource,
                        };
                        if (!_textureCache.TryGetValue(cacheKey, out Texture texture))
                        {
                            texture = Tex.TextureTo(
                                LoadTexture(coloredIconPayload),
                                coloredIconPayload.Color.GetColor(),
                                -1,
                                Mathf.FloorToInt(position.height)
                            );
                            if(texture.width != 1 && texture.height != 1)
                            {
                                _textureCache.Add(cacheKey, texture);
                            }
                        }
                        label = new GUIContent(texture);
                        break;
                    }
                    case RichText.IconPayload iconPayload:
                    {
                        TextureCacheKey cacheKey = new TextureCacheKey
                        {
                            EColor = EColor.White,
                            IconResourcePath = iconPayload.IconResourcePath,
                            IsEditorResource = iconPayload.IsEditorResource,
                        };
                        if (!_textureCache.TryGetValue(cacheKey, out Texture texture))
                        {
                            texture = Tex.TextureTo(
                                LoadTexture(iconPayload),
                                EColor.White.GetColor(),
                                -1,
                                Mathf.FloorToInt(position.height)
                            );
                            if(texture.width != 1 && texture.height != 1)
                            {
                                _textureCache.Add(cacheKey, texture);
                            }
                        }

                        label = new GUIContent(texture);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(richTextPayload), richTextPayload, null);
                }

                float width = labelStyle.CalcSize(label).x;
                (Rect curRect, Rect leftRect) = RectUtils.SplitWidthRect(accPosition, width);
                GUI.Label(curRect, label, labelStyle);
                accPosition = leftRect;
            }
        }

        private static Texture2D LoadTexture(RichText.IconPayload iconPayload)
        {
            return iconPayload.IsEditorResource
                ? (Texture2D)EditorGUIUtility.Load(iconPayload.IconResourcePath)
                : (Texture2D)AssetDatabase.LoadAssetAtPath<Texture>(iconPayload.IconResourcePath);
        }
    }
}
