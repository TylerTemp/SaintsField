using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ExtInspector.Editor.Utils;
using ExtInspector.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
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

        // public void DrawLabel(Rect position, GUIContent oldLabel, IEnumerable<RichText.RichTextPayload> payloads)
        // {
        //     Rect accPosition = position;
        //     foreach (RichText.RichTextPayload richTextPayload in payloads)
        //     {
        //         // Rect curRect;
        //         // Rect leftRect;
        //         GUIStyle labelStyle = EditorStyles.label;
        //         GUIContent label = oldLabel;
        //
        //         switch (richTextPayload)
        //         {
        //             case RichText.ColoredLabelPayload coloredLabelPayload:
        //                 labelStyle = new GUIStyle(GUI.skin.label)
        //                 {
        //                     normal =
        //                     {
        //                         textColor = coloredLabelPayload.Color.GetColor(),
        //                     },
        //                 };
        //                 break;
        //             case RichText.LabelPayload _:
        //                 break;
        //             case RichText.ColoredTextPayload coloredTextPayload:
        //                 labelStyle = new GUIStyle(GUI.skin.label)
        //                 {
        //                     normal =
        //                     {
        //                         textColor = coloredTextPayload.Color.GetColor(),
        //                     },
        //                 };
        //                 label = new GUIContent(coloredTextPayload.Text);
        //                 break;
        //             case RichText.TextPayload textPayload:
        //                 label = new GUIContent(textPayload.Text);
        //                 break;
        //             case RichText.ColoredIconPayload coloredIconPayload:
        //             {
        //                 TextureCacheKey cacheKey = new TextureCacheKey
        //                 {
        //                     EColor = coloredIconPayload.Color,
        //                     IconResourcePath = coloredIconPayload.IconResourcePath,
        //                     IsEditorResource = coloredIconPayload.IsEditorResource,
        //                 };
        //                 if (!_textureCache.TryGetValue(cacheKey, out Texture texture))
        //                 {
        //                     texture = Tex.TextureTo(
        //                         LoadTexture(coloredIconPayload),
        //                         coloredIconPayload.Color.GetColor(),
        //                         -1,
        //                         Mathf.FloorToInt(position.height)
        //                     );
        //                     if(texture.width != 1 && texture.height != 1)
        //                     {
        //                         _textureCache.Add(cacheKey, texture);
        //                     }
        //                 }
        //                 label = new GUIContent(texture);
        //                 break;
        //             }
        //             case RichText.IconPayload iconPayload:
        //             {
        //                 TextureCacheKey cacheKey = new TextureCacheKey
        //                 {
        //                     EColor = EColor.White,
        //                     IconResourcePath = iconPayload.IconResourcePath,
        //                     IsEditorResource = iconPayload.IsEditorResource,
        //                 };
        //                 if (!_textureCache.TryGetValue(cacheKey, out Texture texture))
        //                 {
        //                     texture = Tex.TextureTo(
        //                         LoadTexture(iconPayload),
        //                         EColor.White.GetColor(),
        //                         -1,
        //                         Mathf.FloorToInt(position.height)
        //                     );
        //                     if(texture.width != 1 && texture.height != 1)
        //                     {
        //                         _textureCache.Add(cacheKey, texture);
        //                     }
        //                 }
        //
        //                 label = new GUIContent(texture);
        //                 break;
        //             }
        //
        //             default:
        //                 throw new ArgumentOutOfRangeException(nameof(richTextPayload), richTextPayload, null);
        //         }
        //
        //         float width = labelStyle.CalcSize(label).x;
        //         (Rect curRect, Rect leftRect) = RectUtils.SplitWidthRect(accPosition, width);
        //         GUI.Label(curRect, label, labelStyle);
        //         accPosition = leftRect;
        //     }
        // }

        public void DrawChunks(Rect position, GUIContent oldLabel, IEnumerable<RichTextChunk> payloads)
        {
            Rect labelRect = position;
            // List<RichTextChunk> parsedChunk = payloads.ToList();

            // Debug.Log($"parsedChunk.Count={parsedChunk.Count}");

            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
            };
            GUIContent curGUIContent;
            float curWidth;

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
                // bool noMoreSpace = curWidth >= labelRect.width;
                // if (noMoreSpace)
                // {
                //     textRect = labelRect;
                // }
                GUI.Label(textRect, curGUIContent, textStyle);
                // if (noMoreSpace)
                // {
                //     // Debug.LogWarning("no more space, break");
                //     return;
                // }

                labelRect = leftRect;
            }
        }

        private const string EditorFolderName =
#if EXT_INSPECTOR_IN_DEV
                "Assets/ExtInspector/Editor/Editor Default Resources/ExtInspector/"
#else
                "Packages/today.comes.extinspector/Editor/Editor Default Resources/ExtInspector/"
#endif
            ;

        private static Texture2D LoadTexture(string iconPath)
        {
            Texture2D result = new[]
            {
                iconPath,
                EditorFolderName + iconPath,
            }
                .Select(each => (Texture2D)EditorGUIUtility.Load(each))
                .FirstOrDefault(each => each != null);

            Debug.Assert(result != null, $"{iconPath} not found in default or {EditorFolderName}");
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

        // NOTE: Unity rich text is NOT xml
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
            List<RichTextChunk> richTextChunks = new List<RichTextChunk>();
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
                                    richTextChunks.Add(new RichTextChunk
                                    {
                                        IsIcon = false,
                                        Content = $"{curContent}{endTagsString}",
                                    });
                                }
#if EXT_INSPECTOR_LOG
                                Debug.Log($"chunk added icon {parsedResult.value}");
#endif

                                richTextChunks.Add(new RichTextChunk
                                {
                                    IsIcon = true,
                                    Content = parsedResult.value,
                                    IconColor = colors.Count > 0 ? colors[^1] : null,
                                });

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
            if (leftContent != "")
            {
                string endTagsString = string.Join("", openTags.Select(each => $"</{each.tagName}>").Reverse());
#if EXT_INSPECTOR_LOG
                Debug.Log($"chunk added left: {leftContent}{endTagsString}");
#endif
                richTextChunks.Add(new RichTextChunk
                {
                    IsIcon = false,
                    Content = $"{leftContent}{endTagsString}",
                });
            }

            return richTextChunks;
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
                if(endTagRawContent.Length > 0)
                {
                    return (RichPartType.EndTag, endTagRawContent.Trim(), null, false);
                }
                return (RichPartType.Content, part, null, false);
            }
            if (part.EndsWith("/>"))  // self close
            {
                string endTagRawContent = part.Substring(1, part.Length - 3).Trim();
                (string endTagName, string endTagValue) = ParseTag(endTagRawContent);
                if(endTagName.Length > 0)
                {
                    return (RichPartType.EndTag, endTagName, endTagValue, true);
                }
                return (RichPartType.Content, part, null, false);
            }

            // open tag
            string tagRawContent = part.Substring(1, part.Length - 2);
            (string tagName, string tagValue) = ParseTag(tagRawContent);
            return tagName.Length > 0 ? (RichPartType.StartTag, tagName, tagValue, false) : (RichPartType.Content, part, null, false);
        }

        private static (string tagName, string tagValue) ParseTag(string tagRaw)
        {
            const string reg = @"(\w+)=(.+)";
            var match = Regex.Match(tagRaw, reg);
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

        private enum BlockChunkType
        {
            None,
            Tag,
            BareTagValue,
            SingleQuoteTagValue,
            DoubleQuoteTagValue,
        }

        private enum BlockType
        {
            Continue,
            Content,
            Tag,
            TagWithValue,
        }

        private struct Block
        {
            public BlockType BlockType;
            public string Value;
            public bool IsTagStop;
            public string TagArgs;
        }

        // private static Block AccBlock(char curChar, Stack<BlockChunkType> blocks, List<char> tagNames, List<char> tagValues, List<char> contents)
        // {
        //     BlockChunkType blockChunkType = blocks.TryPeek(out BlockChunkType lastBlockType) ? lastBlockType : BlockChunkType.None;
        //     switch (curChar)
        //     {
        //         case '<':
        //             switch (blockChunkType)
        //             {
        //                 case BlockChunkType.None:
        //                     blocks.Push(BlockChunkType.Tag);
        //                     return new Block();
        //                 case BlockChunkType.Tag:
        //                     // 解析异常，将前一个<视为普通字符
        //                     contents.Add('<');
        //                     return new Block();
        //                 case BlockChunkType.BareTagValue:
        //                 case BlockChunkType.SingleQuoteTagValue:
        //                 case BlockChunkType.DoubleQuoteTagValue:
        //                     tagValues.Add(curChar);
        //                     return new Block();
        //                 default:
        //                     throw new ArgumentOutOfRangeException();
        //             }
        //         case '>':
        //             switch (blockChunkType)
        //             {
        //                 case BlockChunkType.None:
        //                     contents.Add(curChar);
        //                     return new Block();
        //                 case BlockChunkType.Tag:
        //                     blocks.Pop();
        //                     if (tagNames.Count == 0)  // <>
        //                     {
        //                         contents.Add('<');
        //                         contents.Add('>');
        //                         return new Block();
        //                     }
        //                     else if (tagNames[0] == '/' || tagNames[^1] == '/')  // close
        //                     {
        //                         string tagValue = null;
        //                         if (tagValues.Count > 0)
        //                         {
        //                             Debug.Assert(tagValues[0] == '=');
        //                             tagValues.RemoveAt(0);
        //                             if (tagValues.Count == 0)
        //                             {
        //                                 tagValue = "";
        //                             }
        //                             else
        //                             {
        //                                 if ((tagValues[0] == '"' && tagValues[^1] == '"') ||
        //                                     (tagValues[0] == '\\' && tagValues[^1] == '\\'))
        //                                 {
        //                                     tagValues.RemoveAt(0);
        //                                     tagValues.RemoveAt(tagValues.Count - 1);
        //                                 }
        //                                 tagValue = new string(tagValues.ToArray());
        //                             }
        //                         }
        //                         tagValues.Clear();
        //
        //                         if (tagNames[0] == '/')
        //                         {
        //
        //                         }
        //
        //                         Block block = new Block
        //                         {
        //                             BlockType = tagValue==null? BlockType.Tag: BlockType.TagWithValue,
        //                             IsTagStop = true,
        //                             TagArgs = tagValue,
        //                             Value =
        //                         }
        //                     }
        //
        //                     break;
        //                 case BlockChunkType.BareTagValue:
        //                     break;
        //                 case BlockChunkType.SingleQuoteTagValue:
        //                     break;
        //                 case BlockChunkType.DoubleQuoteTagValue:
        //                     break;
        //                 default:
        //                     throw new ArgumentOutOfRangeException();
        //             }
        //             break;
        //         case '\'':
        //             switch (blockChunkType)
        //             {
        //                 case BlockChunkType.None:
        //                     contents.Add(curChar);
        //                     return new Block();
        //                 case BlockChunkType.Tag:
        //                     break;
        //                 case BlockChunkType.BareTagValue:
        //                     break;
        //                 case BlockChunkType.SingleQuoteTagValue:
        //                     break;
        //                 case BlockChunkType.DoubleQuoteTagValue:
        //                     break;
        //                 default:
        //                     throw new ArgumentOutOfRangeException();
        //             }
        //             break;
        //         case '"':
        //             if (blockChunkType == BlockChunkType.None)
        //             {
        //                 blocks.Push(BlockChunkType.DoubleQuoteTagValue);
        //             }
        //             else if (blockChunkType == BlockChunkType.DoubleQuoteTagValue)
        //             {
        //                 blocks.Push(BlockChunkType.None);
        //             }
        //             break;
        //         default:
        //             if (blockChunkType == BlockChunkType.None)
        //             {
        //                 blocks.Push(BlockChunkType.BareTagValue);
        //             }
        //             break;
        //     }
        // }
    }
}
