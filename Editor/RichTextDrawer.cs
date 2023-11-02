using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SaintsField.Utils;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    public class RichTextDrawer: IDisposable
    {
        // cache
        private struct TextureCacheKey
        {
            public string ColorPresent;
            public string IconPath;
            // public bool IsEditorResource;

            public override bool Equals(object obj)
            {
                if (obj is not TextureCacheKey other)
                {
                    return false;
                }

                return ColorPresent == other.ColorPresent && IconPath == other.IconPath;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = ColorPresent != null ? ColorPresent.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (IconPath != null ? IconPath.GetHashCode() : 0);
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

        private static Texture2D LoadTexture(string iconPath)
        {
            string[] paths = {
                iconPath,
                "Assets/SaintsField/Editor/Editor Default Resources/SaintsField/" + iconPath,
                "SaintsField/" + iconPath,
                // this is readonly, put it to last so user  can easily override it
                "Packages/today.comes.saintsfield/Editor/Editor Default Resources/SaintsField/" + iconPath,
            };

            Texture2D result = paths
                .Select(each => (Texture2D)EditorGUIUtility.Load(each))
                .FirstOrDefault(each => each != null);

            Debug.Assert(result != null, $"{iconPath} not found in {string.Join(", ", paths)}");
            return result;
            // return iconPayload.IsEditorResource
            //     ? (Texture2D)EditorGUIUtility.Load(iconPayload.IconResourcePath)
            //     : (Texture2D)AssetDatabase.LoadAssetAtPath<Texture>(iconPayload.IconResourcePath);
        }

        public struct RichTextChunk
        {
            public bool IsIcon;
            public string Content;
            public string IconColor;

            public override string ToString() => IsIcon
                ? $"<ICON={Content} COLOR={IconColor}/>"
                : Content;
        }

        // e.g. prefix<icon=iconPath/>some <color=red>rich</color> <color=green><label /></color>suffix
        // also ok: prefix<color=red>some <color=green><b>[<icon=path/><label />]</b></color>suffix</color>
        // so, only special process <icon /> and <label />

        // NOTE: Unity rich text is NOT xml; This is not Unity rich text as
        // Unity will treat invalid rich text as plain text. This will try to fix the broken xml
        public static IEnumerable<RichTextChunk> ParseRichXml(string richXml, string labelText)
        {
            List<string> colors = new List<string>();

            // Define a regular expression pattern to match the tags
            const string pattern = "(<[^>]+>)";

            // Use Regex.Split to split the string by tags
            string[] splitByTags = Regex.Split(richXml, pattern);

            // List<string> colorPresent = new List<string>();
            // List<string> stringPresent = new List<string>();
            List<(string tagName, string tagValueOrNull, string rawContent)> openTags = new List<(string tagName, string tagValueOrNull, string rawContent)>();
            StringBuilder richText = new StringBuilder();
            // List<RichTextChunk> richTextChunks = new List<RichTextChunk>();
            foreach (string part in splitByTags.Where(each => each != ""))
            {
                (RichPartType partType, string content, string value, bool isSelfClose) parsedResult = ParsePart(part);
#if EXT_INSPECTOR_LOG
                Debug.Log($"get part {part} -> {parsedResult}");
#endif
                switch (parsedResult)
                {
                    case (RichPartType.Content, not null, null, false):
                    {
#if EXT_INSPECTOR_LOG
                        Debug.Log($"string append {parsedResult.content}");
#endif
                        richText.Append(parsedResult.content);
                    }
                        break;
                    case (RichPartType.StartTag, not null, _, false):
                    {
                        openTags.Add((parsedResult.content, parsedResult.value, part));
                        if (parsedResult.content == "color")
                        {
#if EXT_INSPECTOR_LOG
                            Debug.Log($"colors add {parsedResult.value}");
#endif
                            colors.Add(parsedResult.value);
                        }

#if EXT_INSPECTOR_LOG
                        Debug.Log($"string append {part}");
#endif
                        richText.Append(part);
                    }
                        break;
                    case (RichPartType.EndTag, not null, _, _):
                    {
                        if (!parsedResult.isSelfClose)
                        {
                            Debug.Assert(openTags[^1].tagName == parsedResult.content);
                            openTags.RemoveAt(openTags.Count - 1);
                        }

                        switch (parsedResult.content)
                        {
                            case "color" when colors.Count == 0:
                                Debug.LogWarning($"missing open color tag for {richText}");
                                break;
                            case "color" when colors.Count > 0:
#if EXT_INSPECTOR_LOG
                                Debug.Log($"colors remove last");
#endif
                                colors.RemoveAt(colors.Count - 1);
#if EXT_INSPECTOR_LOG
                                Debug.Log($"string append {part}");
#endif
                                richText.Append(part);
                                break;
                            case "label":
#if EXT_INSPECTOR_LOG
                                Debug.Log($"string append {labelText}");
#endif
                                richText.Append(labelText);
                                break;
                            case "icon":
                            {
                                Debug.Assert(parsedResult.value != null);
                                // process ending
                                string curContent = richText.ToString();
                                if (curContent != "")
                                {
                                    string endTagsString = string.Join("", openTags.Select(each => $"</{each.tagName}>").Reverse());
#if EXT_INSPECTOR_LOG
                                    Debug.Log($"chunk added {curContent}{endTagsString}");
#endif
                                    yield return new RichTextChunk
                                    {
                                        IsIcon = false,
                                        Content = $"{curContent}{endTagsString}",
                                    };
                                }
#if EXT_INSPECTOR_LOG
                                Debug.Log($"chunk added icon {parsedResult.value}");
#endif

                                yield return new RichTextChunk
                                {
                                    IsIcon = true,
                                    Content = parsedResult.value,
                                    IconColor = colors.Count > 0 ? colors[^1] : null,
                                };

                                string textOpeningTags = string.Join("", openTags.Select(each => each.rawContent));
#if EXT_INSPECTOR_LOG
                                Debug.Log($"string new with {textOpeningTags}");
#endif
                                richText = new StringBuilder(textOpeningTags);
                            }
                                break;
                            default:
                            {
#if EXT_INSPECTOR_LOG
                                Debug.Log($"default string append {part} for {parsedResult}");
#endif
                                richText.Append(part);
                            }
                                break;
                        }
                    }
                        break;
                }
            }

            string leftContent = richText.ToString();

            // ReSharper disable once InvertIf
            if (leftContent != "")
            {
                string endTagsString = string.Join("", openTags.Select(each => $"</{each.tagName}>").Reverse());
#if EXT_INSPECTOR_LOG
                Debug.Log($"chunk added left: {leftContent}{endTagsString}");
#endif
                yield return new RichTextChunk
                {
                    IsIcon = false,
                    Content = $"{leftContent}{endTagsString}",
                };
            }

            // return richTextChunks;
        }

        private enum RichPartType
        {
            Content,
            StartTag,
            EndTag,
        }

        private static (RichPartType partType, string content, string value, bool isSelfClose) ParsePart(string part)
        {
            if (!part.StartsWith("<") || !part.EndsWith(">"))  // content
            {
                return (RichPartType.Content, part, null, false);
            }

            if (part.StartsWith("</"))  // close
            {
                string endTagRawContent = part.Substring(2, part.Length - 3).Trim();
                return endTagRawContent.Length > 0
                    ? (RichPartType.EndTag, endTagRawContent.Trim(), null, false)
                    : (RichPartType.Content, part, null, false);
            }
            if (part.EndsWith("/>"))  // self close
            {
                string endTagRawContent = part.Substring(1, part.Length - 3).Trim();
                (string endTagName, string endTagValue) = ParseTag(endTagRawContent);
                return endTagName.Length > 0
                    ? (RichPartType.EndTag, endTagName, endTagValue, true)
                    : (RichPartType.Content, part, null, false);
            }

            // open tag
            string tagRawContent = part.Substring(1, part.Length - 2);
            (string tagName, string tagValue) = ParseTag(tagRawContent);
            return tagName.Length > 0
                ? (RichPartType.StartTag, tagName, tagValue, false)
                : (RichPartType.Content, part, null, false);
        }

        private static (string tagName, string tagValue) ParseTag(string tagRaw)
        {
            const string reg = @"(\w+)=(.+)";
            Match match = Regex.Match(tagRaw, reg);
            if (!match.Success)
            {
                return (tagRaw.Trim(), null);
            }

            string tagName = match.Groups[1].Value;
            string tagValue = match.Groups[2].Value;
            if ((tagValue.StartsWith("'") && tagValue.EndsWith("'")) ||
                (tagValue.StartsWith("\"") && tagValue.EndsWith("\"")))
            {
                tagValue = tagValue.Substring(1, tagValue.Length - 2);
            }

            return (tagName.Trim(), tagValue);
        }

        public float GetWidth(GUIContent oldLabel, float height, IEnumerable<RichTextChunk> payloads)
        {
            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
            };

            float totalWidth = 0;

            foreach(RichTextChunk curChunk in payloads)
            {
                // RichTextChunk curChunk = parsedChunk[0];
                // parsedChunk.RemoveAt(0);

                // Debug.Log($"parsedChunk={curChunk}");
                if (curChunk.IsIcon)
                {
                    TextureCacheKey cacheKey = new TextureCacheKey
                    {
                        ColorPresent = curChunk.IconColor,
                        IconPath = curChunk.Content,
                    };
                    Texture texture = GetTexture2D(cacheKey, curChunk, height);
                    float curWidth = texture.height > 0
                        ? texture.width
                        : height;

                    totalWidth += curWidth;
                }
                else
                {
                    GUIContent curGUIContent = new GUIContent(oldLabel)
                    {
                        text = curChunk.Content,
                        image = null,
                    };
                    totalWidth += textStyle.CalcSize(curGUIContent).x;
                }
            }
            return totalWidth;
        }

        public void DrawChunks(Rect position, GUIContent oldLabel, IEnumerable<RichTextChunk> payloads)
        {
            Rect labelRect = position;
            // List<RichTextChunk> parsedChunk = payloads.ToList();

            // Debug.Log($"parsedChunk.Count={parsedChunk.Count}");

            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
            };

            foreach(RichTextChunk curChunk in payloads)
            {
                // RichTextChunk curChunk = parsedChunk[0];
                // parsedChunk.RemoveAt(0);

                // Debug.Log($"parsedChunk={curChunk}");
                GUIContent curGUIContent;
                float curWidth;
                if (curChunk.IsIcon)
                {
                    TextureCacheKey cacheKey = new TextureCacheKey
                    {
                        ColorPresent = curChunk.IconColor,
                        IconPath = curChunk.Content,
                    };
                    if (!_textureCache.TryGetValue(cacheKey, out Texture texture))
                    {
                        texture = Tex.TextureTo(
                            LoadTexture(curChunk.Content),
                            Colors.GetColorByStringPresent(curChunk.IconColor),
                            -1,
                            Mathf.FloorToInt(position.height)
                        );
                        if (texture.width != 1 && texture.height != 1)
                        {
                            _textureCache.Add(cacheKey, texture);
                        }
                    }

#if EXT_INSPECTOR_LOG
                    Debug.Log($"#draw# icon <{curChunk.Content} {curChunk.IconColor}/>");
#endif
                    curGUIContent = new GUIContent(oldLabel)
                    {
                        text = null,
                        image = texture,
                    };
                    curWidth = texture.width;
                }
                else
                {
                    curGUIContent = new GUIContent(oldLabel)
                    {
                        text = curChunk.Content,
                        image = null,
                    };
                    curWidth = textStyle.CalcSize(curGUIContent).x;
                }

                (Rect textRect, Rect leftRect) = RectUtils.SplitWidthRect(labelRect, curWidth);
                GUI.Label(textRect, curGUIContent, textStyle);

                labelRect = leftRect;
            }
        }

        private Texture GetTexture2D(TextureCacheKey cacheKey, RichTextChunk curChunk, float height)
        {
            if (_textureCache.TryGetValue(cacheKey, out Texture texture))
            {
                return texture;
            }

            texture = Tex.TextureTo(
                LoadTexture(curChunk.Content),
                Colors.GetColorByStringPresent(curChunk.IconColor),
                -1,
                Mathf.FloorToInt(height)
            );
            if (texture.width != 1 && texture.height != 1)
            {
                _textureCache.Add(cacheKey, texture);
            }

            return texture;
        }
    }
}
