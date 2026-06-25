using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Core
{
    public class RichTextDrawer
    {
        // cache
        private struct TextureCacheKey : IEquatable<TextureCacheKey>
        {
            public string ColorPresent;
            public string IconPath;
            // public bool IsEditorResource;

            public override bool Equals(object obj)
            {
                // ReSharper disable once UseNegatedPatternInIsExpression
                if (!(obj is TextureCacheKey other))
                {
                    return false;
                }

                return ColorPresent == other.ColorPresent && IconPath == other.IconPath;
            }

            public override int GetHashCode()
            {
                if (ColorPresent == null)
                {
                    return IconPath != null ? IconPath.GetHashCode() : 0;
                }

                return Util.CombineHashCode(ColorPresent, IconPath);
            }

            public bool Equals(TextureCacheKey other)
            {
                return ColorPresent == other.ColorPresent && IconPath == other.IconPath;
            }
        }

        // private readonly Dictionary<TextureCacheKey, Texture2D> _textureCache = new Dictionary<TextureCacheKey, Texture2D>();
        //
        // public void Dispose()
        // {
        //     foreach (Texture2D cacheValue in _textureCache.Values)
        //     {
        //         UnityEngine.Object.DestroyImmediate(cacheValue);
        //     }
        //     _textureCache.Clear();
        // }

        public static (string error, string xml) GetLabelXml(SerializedProperty property, string richTextXml, bool isCallback, FieldInfo fieldInfo, object target)
        {
            if (!isCallback)
            {
                return ("", richTextXml);
            }

            (string error, MemberInfo _, string result) = Util.GetOf(richTextXml, "", property, fieldInfo, target, null);
            if (error != "")
            {
                string originalName;
                try
                {
                    originalName = property.displayName;
                }
                catch(InvalidOperationException e)
                {
                    return (e.Message, "");
                }

                return (error, originalName);
            }

            return ("", result);
        }

        public readonly struct RichTextChunk: IEquatable<RichTextChunk>
        {
            // ReSharper disable once MemberCanBePrivate.Global
            public readonly string RawContent;

            public readonly bool IsIcon;
            public readonly string Content;
            public readonly string IconColor;

            public RichTextChunk(string rawContent = "", bool isIcon = false, string content = "", string iconColor = null)
            {
                RawContent = rawContent ?? "";
                IsIcon = isIcon;
                Content = content ?? "";
                IconColor = iconColor;
            }

            public override string ToString() => IsIcon
                ? $"<ICON={Content} COLOR={IconColor}/>"
                : Content.Replace("<", "[").Replace(">", "]");

            public bool Equals(RichTextChunk other)
            {
                return RawContent == other.RawContent && IsIcon == other.IsIcon && Content == other.Content && IconColor == other.IconColor;
            }

            public override bool Equals(object obj)
            {
                return obj is RichTextChunk other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(RawContent, IsIcon, Content, IconColor);
            }
        }

        // e.g. prefix<icon=iconPath/>some <color=red>rich</color> <color=green><label /></color>suffix
        // also ok: prefix<color=red>some <color=green><b>[<icon=path/><label />]</b></color>suffix</color>
        // so, only special process <icon /> and <label />

        // NOTE: Unity rich text is NOT xml; This is not Unity rich text as
        // Unity will treat invalid rich text as plain text. This will try to fix the broken xml
//         [Obsolete]
//         public static IEnumerable<RichTextChunk> ParseRichXml(string richXml, string labelText, SerializedProperty property, MemberInfo fieldInfo, object parent)
//         {
//             List<string> colors = new List<string>();
//
//             // // Define a regular expression pattern to match the tags
//             // const string pattern = "(<[^>]+>)";
//             //
//             // // Use Regex.Split to split the string by tags
//             // string[] splitByTags = Regex.Split(richXml, pattern);
//             string[]  splitByTags = RuntimeUtil.SplitByTags(richXml).Select(each => each.stringChunk).ToArray();
//
//             // List<string> colorPresent = new List<string>();
//             // List<string> stringPresent = new List<string>();
//             List<(string tagName, string tagValueOrNull, string rawContent)> openTags = new List<(string tagName, string tagValueOrNull, string rawContent)>();
//             StringBuilder richText = new StringBuilder();
//             // List<RichTextChunk> richTextChunks = new List<RichTextChunk>();
//             foreach (string part in splitByTags.Where(each => each != ""))
//             {
//                 (RichPartType partType, string content, string value, bool isSelfClose) parsedResult = ParsePart(part);
//
//                 // Debug.Log($"parse: {part} -> {parsedResult.partType}, {parsedResult.content}, {parsedResult.value}, {parsedResult.isSelfClose}");
//
//                 // ReSharper disable once MergeIntoPattern
//                 // ReSharper disable once ConvertIfStatementToSwitchStatement
//                 if (parsedResult.partType == RichPartType.Content && parsedResult.value == null && !parsedResult.isSelfClose)
//                 {
//                     richText.Append(parsedResult.content);
//                 }
//                 // ReSharper disable once MergeIntoPattern
//                 else if (parsedResult.partType == RichPartType.StartTag && !parsedResult.isSelfClose)
//                 {
//                     // Debug.Log($"parse={parsedResult.content}, {parsedResult.value}");
//                     openTags.Add((parsedResult.content, parsedResult.value, part));
//                     if (parsedResult.content == "color")
//                     {
//                         colors.Add(parsedResult.value);
//                         // richText.Append(Colors.ColorNameSupported(parsedResult.value)
//                         //     ? part
//                         //     : $"<color={Colors.ToHtmlHexString(Colors.GetColorByStringPresent(parsedResult.value))}>");
//                         // richText.Append(
//                         //     $"<color={Colors.ToHtmlHexString(Colors.GetColorByStringPresent(parsedResult.value))}>");、
//                         richText.Append(
//                             $"<color={parsedResult.value}>");
//                     }
//                     else
//                     {
//                         richText.Append(part);
//                     }
//                 }
//                 else if (parsedResult.partType == RichPartType.EndTag)
//                 {
//                     if (!parsedResult.isSelfClose)
//                     {
//                         // ReSharper disable once UseIndexFromEndExpression
// #if SAINTSFIELD_DEBUG
//                         Debug.Assert(openTags[openTags.Count - 1].tagName == parsedResult.content, parsedResult.content);
// #endif
//                         if(openTags.Count > 0)
//                         {
//                             openTags.RemoveAt(openTags.Count - 1);
//                         }
//                     }
//
//                     switch (parsedResult.content)
//                     {
//                         case "color" when colors.Count == 0:
// #if SAINTSFIELD_DEBUG
//                             Debug.LogError($"missing open color tag for {richText}");
// #endif
//                             break;
//                         case "color" when colors.Count > 0:
//                             colors.RemoveAt(colors.Count - 1);
//
//                             richText.Append(part);
//                             break;
//                         case "label":
//                             richText.Append(labelText);
//                             break;
//                         case "container.Type":
//                         {
//                             Type decType = fieldInfo?.DeclaringType;
//                             richText.Append(decType == null ? "" : decType.Name);
//                         }
//                             break;
//                         case "container.Type.BaseType":
//                         {
//                             Type baseType = fieldInfo?.DeclaringType?.BaseType;
//                             richText.Append(baseType == null? "": baseType.Name);
//                         }
//                             break;
//                         case "index":
//                         {
//                             if (property != null && SerializedUtils.IsOk(property))
//                             {
//                                 int index = SerializedUtils.PropertyPathIndex(property.propertyPath);
//                                 if (index >= 0)
//                                 {
//                                     richText.Append(TagStringFormatter(index, parsedResult.value));
//                                 }
//                             }
//                         }
//                             break;
//                         case "icon":
//                         {
//                             Debug.Assert(parsedResult.value != null);
//                             // process ending
//                             string curContent = richText.ToString();
//                             if (curContent != "")
//                             {
//                                 string endTagsString = string.Join("", openTags.Select(each => $"</{each.tagName}>").Reverse());
//                                 yield return new RichTextChunk(isIcon: false, content: $"{curContent}{endTagsString}");
//                                 // {
//                                 //     IsIcon = false,
//                                 //     Content = $"{curContent}{endTagsString}",
//                                 // };
//                             }
//
//                             yield return new RichTextChunk(isIcon: true, content: parsedResult.value,
//                                 iconColor: colors.Count > 0 ? colors[colors.Count - 1] : null);
//                             // {
//                             //     IsIcon = true,
//                             //     Content = parsedResult.value,
//                             //     // ReSharper disable once UseIndexFromEndExpression
//                             //     IconColor = colors.Count > 0 ? colors[colors.Count - 1] : null,
//                             // };
//
//                             // string textOpeningTags = string.Join("", openTags.Select(each => each.rawContent));
//                             string textOpeningTags = string.Join("", openTags.Select(each => $"<{each.tagName}{(each.tagValueOrNull==null? "": $"={each.tagValueOrNull}")}>"));
//                             richText = new StringBuilder(textOpeningTags);
//                         }
//                             break;
//                         default:
//                         {
//                             // Debug.Log(parsedResult.content);
//                             if (parsedResult.content != null && (parsedResult.content == "field" ||
//                                                                  parsedResult.content.StartsWith("field.")))
//                             {
//                                 (string error, int index, object value) fieldValue = Util.GetValue(property, fieldInfo, parent);
//                                 if (fieldValue.error != "")
//                                 {
// #if SAINTSFIELD_DEBUG
//                                     Debug.LogError(fieldValue.error);
// #endif
//                                 }
//                                 else
//                                 {
//                                     object finalValue = null;
//                                     bool hasError = false;
//                                     if(parsedResult.content == "field")
//                                     {
//                                         // richText.Append(RuntimeUtil.IsNull(fieldValue.value)
//                                         //     ? ""
//                                         //     : $"{fieldValue.value}");
//                                         finalValue = fieldValue.value;
//                                     }
//                                     else
//                                     {
//                                         object accParent = fieldValue.value;
//                                         // Debug.Log(parsedResult.content);
//                                         (string error, int index, object value) accResult = ("Field value is null", -1, null);
//                                         if(!RuntimeUtil.IsNull(accParent))
//                                         {
//                                             // ReSharper disable once ReplaceSubstringWithRangeIndexer
//                                             string[] subFields = parsedResult.content.Substring("field.".Length).Split(SerializedUtils.DotSplitSeparator);
//                                             foreach (string attrName in subFields)
//                                             {
//                                                 MemberInfo accMemberInfo = null;
//                                                 foreach (Type type in ReflectUtils.GetSelfAndBaseTypesFromInstance(accParent))
//                                                 {
//                                                     foreach (MemberInfo info in type.GetMember(attrName,
//                                                                  BindingFlags.Public | BindingFlags.NonPublic |
//                                                                  BindingFlags.Instance | BindingFlags.Static |
//                                                                  BindingFlags.FlattenHierarchy))
//                                                     {
//                                                         if (info == null) continue;
//                                                         accMemberInfo = info;
//                                                         break;
//                                                     }
//                                                 }
//
//                                                 accResult = Util.GetValueAtIndex(-1, accMemberInfo, accParent);
//                                                 if (accResult.error != "")
//                                                 {
// #if SAINTSFIELD_DEBUG
//                                                     Debug.LogError($"{attrName} from {accParent}: {accResult.error}");
// #endif
//                                                     break;
//                                                 }
//
//                                                 accParent = accResult.value;
//                                                 if (accParent == null)
//                                                 {
//                                                     accResult = ($"No target found for {attrName}", -1, null);
//                                                     break;
//                                                 }
//                                             }
//                                         }
//
//                                         if (accResult.error == "")
//                                         {
//                                             // Debug.Log($"{parsedResult.content}: {accResult.value}");
//                                             // Debug.Log($"accResult.value={accResult.value}");
//                                             finalValue = accResult.value;
//                                         }
//                                         else
//                                         {
//                                             hasError = true;
//                                         }
//                                         //
//                                         // Debug.Log(string.Join(".", subFields));
//                                     }
//
//                                     if (!hasError)
//                                     {
//                                         string tagFinalResult = TagStringFormatter(finalValue, parsedResult.value);
//                                         richText.Append(tagFinalResult);
//                                     }
//                                 }
//                             }
//                             else
//                             {
//                                 richText.Append(part);
//                             }
//                         }
//                             break;
//                     }
//                 }
//
//             }
//
//             string leftContent = richText.ToString();
//
//             // ReSharper disable once InvertIf
//             if (leftContent != "")
//             {
//                 string endTagsString = string.Join("", openTags.Select(each => $"</{each.tagName}>").Reverse());
// #if EXT_INSPECTOR_LOG
//                 Debug.Log($"chunk added left: {leftContent}{endTagsString}");
// #endif
//                 yield return new RichTextChunk(isIcon: false, content: $"{leftContent}{endTagsString}");
//                 // {
//                 //     IsIcon = false,
//                 //     Content = $"{leftContent}{endTagsString}",
//                 // };
//             }
//
//             // return richTextChunks;
//         }

        public readonly struct EmptyRichTextTagProvider: IRichTextTagProvider
        {
            private readonly string _label;

            public EmptyRichTextTagProvider(string label)
            {
                _label = label;
            }

            public string GetLabel() => _label ?? "";

            public string GetContainerType() => "";

            public string GetContainerTypeBaseType() => "";

            public string GetIndex(string formatter) => "";

            public string GetField(string rawContent, string tagName, string tagValue) => "";
        }

        public static IEnumerable<RichTextChunk> ParseRichXmlWithProvider(string richXml, IRichTextTagProvider provider)
        {
            List<RuntimeUtil.RichTextParsedChunk> openTag = new List<RuntimeUtil.RichTextParsedChunk>();
            // List<RuntimeUtil.RichTextParsedChunk> acc = new List<RuntimeUtil.RichTextParsedChunk>();
            StringBuilder richText = new StringBuilder();
            // Debug.Log($"ParseRichXmlWithProvider `{richXml}`");
            foreach (RuntimeUtil.RichTextParsedChunk richTextParsedChunk in RuntimeUtil.ParseRichXml(richXml))
            {
                // Debug.Log($"get parsed chunk {richTextParsedChunk}");
                // continue;
                switch (richTextParsedChunk.ChunkType)
                {
                    case RuntimeUtil.ChunkType.NormalTag:
                    {
                        bool removed = false;
                        bool isStartTag = richTextParsedChunk.TagType == RuntimeUtil.TagType.StartTag;
                        if (isStartTag)
                        {
                            openTag.Add(richTextParsedChunk);
                        }
                        else
                        {
                            // ReSharper disable once UseIndexFromEndExpression
                            if (openTag.Count > 0 && openTag[openTag.Count - 1].TagName == richTextParsedChunk.TagName)
                            {
                                removed = true;
                                openTag.RemoveAt(openTag.Count - 1);
                            }
                        }

                        switch (richTextParsedChunk.TagName)
                        {
                            case "color":
                            {
                                if(isStartTag)
                                {
                                    string colorHtml =
                                        Colors.ToHtmlHexString(
                                            Colors.GetColorByStringPresent(richTextParsedChunk.TagValue));
                                    richText.Append($"<color={colorHtml}>");
                                }
                                else
                                {
                                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                    if (removed)
                                    {
                                        richText.Append("</color>");
                                    }
                                    else // invalid
                                    {
                                        richText.Append(richTextParsedChunk.RawContent);
                                    }
                                }
                            }
                                break;
                            case "label":
                                richText.Append(provider.GetLabel());
                                break;
                            case "container.Type":
                                richText.Append(provider.GetContainerType());
                                break;
                            case "container.Type.BaseType":
                                richText.Append(provider.GetContainerTypeBaseType());
                                break;
                            case "index":
                                richText.Append(provider.GetIndex(richTextParsedChunk.TagValue));
                                break;
                            case "icon":
                                break;
                            default:
                            {
                                // Debug.Log(parsedResult.content);
                                if (richTextParsedChunk.RawContent != null && (richTextParsedChunk.TagName == "field" ||
                                                                               richTextParsedChunk.TagName.StartsWith("field.")))
                                {
                                    richText.Append(provider.GetField(richTextParsedChunk.RawContent, richTextParsedChunk.TagName,
                                        richTextParsedChunk.TagValue));
                                }
                                else
                                {
                                    richText.Append(richTextParsedChunk.RawContent);
                                }
                            }
                                break;
                        }

                        // Debug.Log($"add tag {richTextParsedChunk}");

                        // acc.Add(richTextParsedChunk);
                        break;
                    }
                    case RuntimeUtil.ChunkType.IconTag:
                    {
                        string richTextFull = richText.ToString();
                        if (richTextFull != "")
                        {
                            yield return new RichTextChunk(richTextFull, false, richTextFull);
                        }
                        richText.Clear();

                        // Debug.Log($"parse icon {richTextParsedChunk}");

                        RichTextChunk wrapIcon = new RichTextChunk(
                            richTextParsedChunk.RawContent,
                            true,
                            richTextParsedChunk.TagValue,
                            richTextParsedChunk.IconColor);
                        // Debug.Log($"add icon {wrapIcon}");
                        // acc.Add(richTextParsedChunk);
                        yield return wrapIcon;
                        break;
                    }
                    case RuntimeUtil.ChunkType.Text:
                        richText.Append(richTextParsedChunk.RawContent);
                        break;
                }
            }

            string richTextFinal = richText.ToString();
            if (richTextFinal != "")
            {
                yield return new RichTextChunk(richTextFinal, false, richTextFinal);
            }
        }

        public static string TagStringFormatter(object finalValue, string parsedResultValue)
        {
            if (RuntimeUtil.IsNull(finalValue))
            {
                // ReSharper disable once TailRecursiveCall
                return TagStringFormatter("", parsedResultValue);
            }

            if (string.IsNullOrEmpty(parsedResultValue))
            {
                return $"{finalValue}";
            }

            if (parsedResultValue.Contains("{") && parsedResultValue.Contains("}"))
            {
                try
                {
                    return string.Format(parsedResultValue, finalValue);
                }
#pragma warning disable CS0168
                catch (Exception ex)
#pragma warning restore CS0168
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(ex);
#endif
                    return $"{finalValue}";
                }
            }

            if (parsedResultValue.StartsWith("B"))
            {
                string binaryFormatResult = Util.FormatBinary(parsedResultValue, finalValue);
                // Debug.Log($"{parsedResult.value}/{finalValue}/{binaryFormatResult}");
                if (binaryFormatResult != "")
                {
                    return binaryFormatResult;
                }
            }

            string formatString = $"{{0:{parsedResultValue}}}";
            try
            {
                return string.Format(formatString, finalValue);
            }
#pragma warning disable CS0168
            catch (Exception ex)
#pragma warning restore CS0168
            {
#if SAINTSFIELD_DEBUG
                // Debug.LogException(ex);
#endif
                return $"{finalValue}";
            }
        }

        // private enum RichPartType
        // {
        //     Content,
        //     StartTag,
        //     EndTag,
        // }

        // private static (RichPartType partType, string content, string value, bool isSelfClose) ParsePart(string part)
        // {
        //     if (!part.StartsWith("<") || !part.EndsWith(">"))  // content
        //     {
        //         return (RichPartType.Content, part, null, false);
        //     }
        //
        //     if (part.StartsWith("</"))  // close
        //     {
        //         string endTagRawContent = part.Substring(2, part.Length - 3).Trim();
        //         if (endTagRawContent.Length > 0)
        //         {
        //             return (RichPartType.EndTag, endTagRawContent.Trim(), null, false);
        //         }
        //         return (RichPartType.Content, part, null, false);
        //     }
        //     if (part.EndsWith("/>"))  // self close
        //     {
        //         string endTagRawContent = part.Substring(1, part.Length - 3).Trim();
        //         (string endTagName, string endTagValue) = ParseTag(endTagRawContent);
        //         return endTagName.Length > 0
        //             ? (RichPartType.EndTag, endTagName, endTagValue, true)
        //             : (RichPartType.Content, part, null, false);
        //     }
        //
        //     // open tag
        //     string tagRawContent = part.Substring(1, part.Length - 2);
        //     (string tagName, string tagValue) = ParseTag(tagRawContent);
        //     return tagName.Length > 0
        //         ? (RichPartType.StartTag, tagName, tagValue, false)
        //         : (RichPartType.Content, part, null, false);
        // }

        // private static (string tagName, string tagValue) ParseTag(string tagRaw)
        // {
        //     // const string reg = @"(\w+)=(.+)";
        //     const string reg = @"([^\=]+)=(.+)";
        //     Match match = Regex.Match(tagRaw, reg);
        //     if (!match.Success)
        //     {
        //         return (tagRaw.Trim(), null);
        //     }
        //
        //     string tagName = match.Groups[1].Value;
        //     string tagValue = match.Groups[2].Value;
        //     if ((tagValue.StartsWith("'") && tagValue.EndsWith("'")) ||
        //         (tagValue.StartsWith("\"") && tagValue.EndsWith("\"")))
        //     {
        //         tagValue = tagValue.Substring(1, tagValue.Length - 2);
        //     }
        //
        //     string tagNameStrip = tagName.Trim();
        //
        //     if (tagNameStrip == "color")
        //     {
        //         tagValue = Colors.ToHtmlHexString(Colors.GetColorByStringPresent(tagValue));
        //         // Debug.Log(tagValue);
        //     }
        //     return (tagNameStrip, tagValue);
        // }

        public float GetWidth(GUIContent oldLabel, float height, IEnumerable<RichTextChunk> payloads)
        {
            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
            };

            float totalWidth = 0;

            foreach (RichTextChunk curChunk in payloads)
            {
                if (curChunk.IsIcon)
                {
                    TextureCacheKey cacheKey = new TextureCacheKey
                    {
                        ColorPresent = curChunk.IconColor,
                        IconPath = curChunk.Content,
                    };
                    Texture texture = GetTexture2DNoTransform(cacheKey, curChunk.Content);
                    if (texture == null)
                    {
                        continue;
                    }
                    // float curWidth = texture.height > 0
                    //     ? texture.width
                    //     : height;

                    totalWidth += height;
                }
                else
                {
                    GUIContent curGUIContent = new GUIContent
                    {
                        text = curChunk.Content,
                    };
                    totalWidth += textStyle.CalcSize(curGUIContent).x;
                }
            }
            return totalWidth;
        }

        private struct GUIGroupScoop : IDisposable
        {
            public GUIGroupScoop(Rect position)
            {
                GUI.BeginGroup(position);
            }


            public void Dispose()
            {
                GUI.EndGroup();
            }
        }

        public void DrawChunks(Rect position, IEnumerable<RichTextChunk> payloads)
        {
            using (new GUIGroupScoop(position))
            {
                Rect leftOutRect = new Rect(position)
                {
                    x = 0,
                    y = 0,
                };


                // Debug.Log($"DrawChunks at {leftOutRect}({leftOutRect.width}x{leftOutRect.height})");

                GUIStyle textStyle = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                };

                foreach (RichTextChunk curChunk in payloads)
                {
                    // Debug.Log($"draw {curChunk} with left {leftOutRect}({leftOutRect.width}x{leftOutRect.height})");
                    if (curChunk.IsIcon)
                    {
                        TextureCacheKey cacheKey = new TextureCacheKey
                        {
                            ColorPresent = curChunk.IconColor,
                            IconPath = curChunk.Content,
                        };

                        Texture2D texture = GetTexture2DNoTransform(cacheKey, curChunk.Content);
                        if (texture == null)
                        {
                            continue;
                        }

                        // if (_textureCache.TryGetValue(cacheKey, out Texture2D texture) && texture != null)
                        // {
                        //     continue;
                        // }
                        //
                        // Texture2D loadTex = Util.LoadResource<Texture2D>(curChunk.Content);
                        // if (loadTex == null || loadTex.width <= 1 || loadTex.height <= 1)
                        // {
                        //     continue;
                        // }
                        // _textureCache[cacheKey] = loadTex;
                        // Debug.Log($"COLOR!={Colors.GetColorByStringPresent(curChunk.IconColor)}/{curChunk.IconColor}");

                        // texture = Tex.TextureTo(
                        //     loadTex,
                        //     Colors.GetColorByStringPresent(curChunk.IconColor),
                        //     -1,
                        //     -1
                        // );
                        // texture = Tex.ApplyTextureColor(loadTex, Colors.GetColorByStringPresent(curChunk.IconColor));
                        // texture = loadTex;
                        // Debug.Log(texture);
                        // Debug.Log(texture == null);
                        // Debug.Log(texture.width);
                        // Debug.Log(texture.height);
                        // _textureCache[cacheKey] = texture;
                        // if (texture.width != 1 && texture.height != 1)
                        // {
                        //     _textureCache[cacheKey] = texture;
                        // }
                        // Texture2D texture = GetTexture2D(cacheKey, curChunk.Content, curChunk.IconColor, position.height);
                        // float curWidth = texture.width;

                        if (leftOutRect.width < position.height)
                        {
                            leftOutRect.width = position.height;
                        }

                        (Rect texRect, Rect leftRect) = RectUtils.SplitWidthRect(leftOutRect, position.height);
                        // Debug.Log($"draw icon {texture} at {textRect}({position})");


                        using(new GUIColorScoop(Colors.GetColorByStringPresent(curChunk.IconColor)))
                        {
                            GUI.DrawTexture(texRect, texture, ScaleMode.ScaleToFit, true);
                        }
                        // EditorGUI.DrawRect(position, Color.green);
                        // EditorGUI.LabelField(textRect, "ok", textStyle);
                        // if (leftRect.width <= 0)
                        // {
                        //     Debug.Log($"No space after icon `{curChunk.Content}, skip");
                        //     return;
                        // }

                        leftOutRect = leftRect;
                        // break;
                    }
                    else
                    {
                        GUIContent curGUIContent = new GUIContent
                        {
                            text = curChunk.Content,
                        };
                        float curWidth = textStyle.CalcSize(curGUIContent).x;
                        if (leftOutRect.width < curWidth)
                        {
                            leftOutRect.width = curWidth;
                        }

                        (Rect textRect, Rect leftRect) = RectUtils.SplitWidthRect(leftOutRect, curWidth);
                        // EditorGUI.DrawRect(textRect, Color.brown);
                        // Debug.Log($"leftRect={leftRect}");
                        EditorGUI.LabelField(textRect, curGUIContent, textStyle);
                        // if (leftRect.width <= 0)
                        // {
                        //     Debug.Log($"No space after text `{curChunk.Content}` ({leftOutRect.width}->{curWidth}), skip");
                        //     return;
                        // }

                        leftOutRect = leftRect;
                    }
                }

            }



        }

        public const float ImageWidth = SaintsPropertyDrawer.SingleLineHeight;
        // public const float ImageWidth = EditorGUIUtility.SingleLineHeight;

#if UNITY_2021_3_OR_NEWER
        public IEnumerable<VisualElement> DrawChunksUIToolKit(IEnumerable<RichTextChunk> payloads)
        {
            foreach(RichTextChunk curChunk in payloads)
            {
                // Debug.Log(curChunk);
                if (!curChunk.IsIcon)
                {
                    yield return new Label(curChunk.Content)
                    {
                        style =
                        {
                            flexShrink = 0,
                            unityTextAlign = TextAnchor.UpperLeft,
                            paddingLeft = 0,
                            paddingRight = 0,
                            whiteSpace = WhiteSpace.Normal,
                        },
                        pickingMode = PickingMode.Ignore,
                    };
                }
                else
                {
                    TextureCacheKey cacheKey = new TextureCacheKey
                    {
                        ColorPresent = "",
                        IconPath = curChunk.Content,
                    };

                    if (!_textureCacheOrigin.TryGetValue(cacheKey, out Texture2D texture) || texture == null)
                    {
                        texture = Util.LoadResource<Texture2D>(curChunk.Content);
                        if (texture != null && texture.width != 1 && texture.height != 1)
                        {
                            _textureCacheOrigin[cacheKey] = texture;
                        }
                    }

                    // Image img = new Image
                    // {
                    //     image = texture,
                    //     scaleMode = ScaleMode.ScaleToFit,
                    //     tintColor = Colors.GetColorByStringPresent(curChunk.IconColor),
                    //     pickingMode = PickingMode.Ignore,
                    //     style =
                    //     {
                    //         flexShrink = 0,
                    //         // marginTop = 2,
                    //         // marginBottom = 2,
                    //         // paddingLeft = 1,
                    //         // paddingRight = 1,
                    //         maxHeight = 15,
                    //         alignSelf = Align.Center,
                    //         width = ImageWidth,
                    //         height = SaintsPropertyDrawer.SingleLineHeight - 2,
                    //     },
                    // };
                    VisualElement img = new VisualElement
                    {
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            backgroundImage = texture,
                            unityBackgroundImageTintColor = Colors.GetColorByStringPresent(curChunk.IconColor),
#if UNITY_2022_2_OR_NEWER
                            backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                            backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                            backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                            backgroundSize = new BackgroundSize(BackgroundSizeType.Contain),
#else
                            unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif

                            flexShrink = 0,
                            // marginTop = 2,
                            // marginBottom = 2,
                            // paddingLeft = 1,
                            // paddingRight = 1,
                            maxHeight = 15,
                            alignSelf = Align.Center,
                            width = ImageWidth,
                            height = SaintsPropertyDrawer.SingleLineHeight - 2,
                        },
                    };
                    // img.style.flexShrink = 0;

#if EXT_INSPECTOR_LOG
                    Debug.Log($"#draw# icon <{curChunk.Content} {curChunk.IconColor}/>");
#endif
                    yield return img;
                }
            }
        }
#endif

        private readonly Dictionary<TextureCacheKey, Texture2D> _textureCacheOrigin = new Dictionary<TextureCacheKey, Texture2D>();

        private Texture2D GetTexture2DNoTransform(TextureCacheKey cacheKey, string iconPath)
        {
            if (_textureCacheOrigin.TryGetValue(cacheKey, out Texture2D texture) && texture != null)
            {
                return texture;
            }

            Texture2D loadTex = Util.LoadResource<Texture2D>(iconPath);
            if (loadTex == null)
            {
                return null;
            }

            return _textureCacheOrigin[cacheKey] = loadTex;
        }

        // private Texture2D GetTexture2D(TextureCacheKey cacheKey, string iconPath, string iconColor, float height)
        // {
        //     if (_textureCache.TryGetValue(cacheKey, out Texture2D texture) && texture != null)
        //     {
        //         return texture;
        //     }
        //
        //     Texture2D loadTex = Util.LoadResource<Texture2D>(iconPath);
        //     if (loadTex == null || loadTex.width <= 1 || loadTex.height <= 1)
        //     {
        //         return null;
        //     }
        //
        //     texture = Tex.TextureTo(
        //         loadTex,
        //         Colors.GetColorByStringPresent(iconColor),
        //         -1,
        //         Mathf.FloorToInt(height)
        //     );
        //     if (texture.width != 1 && texture.height != 1)
        //     {
        //         _textureCache[cacheKey] = texture;
        //     }
        //
        //     return texture;
        // }

// #if UNITY_2021_3_OR_NEWER
//         public static float TextLengthUIToolkit(TextElement calculator, string origin)
//         {
//             // float spaceWidth = calculator.MeasureTextSize(" ", 0, VisualElement.MeasureMode.Undefined, 100, VisualElement.MeasureMode.Undefined).x;
//             // float textWidth = calculator.MeasureTextSize(original, 0, VisualElement.MeasureMode.Undefined, 100, VisualElement.MeasureMode.Undefined).x;
//             // int spaceCount = Mathf.CeilToInt(textWidth / spaceWidth);
//             // return new string(' ', spaceCount);
//
//             return calculator.MeasureTextSize(origin, 0, VisualElement.MeasureMode.Undefined, 100, VisualElement.MeasureMode.Undefined).x;
//
//         }
// #endif
    }
}
