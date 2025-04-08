using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

        private readonly Dictionary<TextureCacheKey, Texture> _textureCache = new Dictionary<TextureCacheKey, Texture>();

        public void Dispose()
        {
            foreach (Texture cacheValue in _textureCache.Values)
            {
                UnityEngine.Object.DestroyImmediate(cacheValue);
            }
            _textureCache.Clear();
        }

        public static (string error, string xml) GetLabelXml(SerializedProperty property, string richTextXml, bool isCallback, FieldInfo fieldInfo, object target)
        {
            if (!isCallback)
            {
                return ("", richTextXml);
            }

            (string error, string result) = Util.GetOf(richTextXml, "", property, fieldInfo, target);
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
            // List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            // types.Reverse();
            // foreach (Type eachType in types)
            // {
            //     (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
            //         ReflectUtils.GetProp(eachType, richTextXml);
            //     switch (getPropType)
            //     {
            //         case ReflectUtils.GetPropType.Field:
            //         {
            //             object result = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
            //             return ("", result == null ? string.Empty : result.ToString());
            //         }
            //
            //         case ReflectUtils.GetPropType.Property:
            //         {
            //             object result = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
            //             return ("", result == null ? string.Empty : result.ToString());
            //         }
            //         case ReflectUtils.GetPropType.Method:
            //         {
            //             MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
            //
            //             int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            //             object rawValue = fieldInfo.GetValue(target);
            //             object curValue = arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex);
            //             object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), arrayIndex == -1
            //                 ? new[]
            //                 {
            //                     curValue,
            //                 }
            //                 : new []
            //                 {
            //                     curValue,
            //                     arrayIndex,
            //                 });
            //
            //             try
            //             {
            //                 return ("", (string)methodInfo.Invoke(
            //                     target,
            //                     passParams
            //                 ));
            //             }
            //             catch (TargetInvocationException e)
            //             {
            //                 Debug.LogException(e);
            //                 Debug.Assert(e.InnerException != null);
            //                 return (e.InnerException.Message, property.displayName);
            //             }
            //             catch (Exception e)
            //             {
            //                 // _error = e.Message;
            //                 Debug.LogException(e);
            //                 return (e.Message, property.displayName);
            //             }
            //         }
            //         case ReflectUtils.GetPropType.NotFound:
            //             continue;
            //         default:
            //             throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            //     }
            // }
            //
            // return ($"not found `{richTextXml}` on `{target}`", property.displayName);
        }

        public struct RichTextChunk
        {
            public string RawContent;

            public bool IsIcon;
            public string Content;
            public string IconColor;

            public override string ToString() => IsIcon
                ? $"<ICON={Content} COLOR={IconColor}/>"
                : Content.Replace("<", "[").Replace(">", "]");
        }

        // e.g. prefix<icon=iconPath/>some <color=red>rich</color> <color=green><label /></color>suffix
        // also ok: prefix<color=red>some <color=green><b>[<icon=path/><label />]</b></color>suffix</color>
        // so, only special process <icon /> and <label />

        // NOTE: Unity rich text is NOT xml; This is not Unity rich text as
        // Unity will treat invalid rich text as plain text. This will try to fix the broken xml
        public static IEnumerable<RichTextChunk> ParseRichXml(string richXml, string labelText, SerializedProperty property, MemberInfo fieldInfo, object parent)
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

                // Debug.Log($"parse: {part} -> {parsedResult.partType}, {parsedResult.content}, {parsedResult.value}, {parsedResult.isSelfClose}");

                // ReSharper disable once MergeIntoPattern
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (parsedResult.partType == RichPartType.Content && parsedResult.value == null && !parsedResult.isSelfClose)
                {
                    richText.Append(parsedResult.content);
                }
                // ReSharper disable once MergeIntoPattern
                else if (parsedResult.partType == RichPartType.StartTag && !parsedResult.isSelfClose)
                {
                    // Debug.Log($"parse={parsedResult.content}, {parsedResult.value}");
                    openTags.Add((parsedResult.content, parsedResult.value, part));
                    if (parsedResult.content == "color")
                    {
                        colors.Add(parsedResult.value);
                        // richText.Append(Colors.ColorNameSupported(parsedResult.value)
                        //     ? part
                        //     : $"<color={Colors.ToHtmlHexString(Colors.GetColorByStringPresent(parsedResult.value))}>");
                        // richText.Append(
                        //     $"<color={Colors.ToHtmlHexString(Colors.GetColorByStringPresent(parsedResult.value))}>");、
                        richText.Append(
                            $"<color={parsedResult.value}>");
                    }
                    else
                    {
                        richText.Append(part);
                    }
                }
                else if (parsedResult.partType == RichPartType.EndTag)
                {
                    if (!parsedResult.isSelfClose)
                    {
                        // ReSharper disable once UseIndexFromEndExpression
#if SAINTSFIELD_DEBUG
                        Debug.Assert(openTags[openTags.Count - 1].tagName == parsedResult.content, parsedResult.content);
#endif
                        if(openTags.Count > 0)
                        {
                            openTags.RemoveAt(openTags.Count - 1);
                        }
                    }

                    switch (parsedResult.content)
                    {
                        case "color" when colors.Count == 0:
#if SAINTSFIELD_DEBUG
                            Debug.LogError($"missing open color tag for {richText}");
#endif
                            break;
                        case "color" when colors.Count > 0:
                            colors.RemoveAt(colors.Count - 1);

                            richText.Append(part);
                            break;
                        case "label":
                            richText.Append(labelText);
                            break;
                        case "container.Type":
                        {
                            Type decType = fieldInfo?.DeclaringType;
                            richText.Append(decType == null ? "" : decType.Name);
                        }
                            break;
                        case "container.Type.BaseType":
                        {
                            Type baseType = fieldInfo?.DeclaringType?.BaseType;
                            richText.Append(baseType == null? "": baseType.Name);
                        }
                            break;
                        case "icon":
                        {
                            Debug.Assert(parsedResult.value != null);
                            // process ending
                            string curContent = richText.ToString();
                            if (curContent != "")
                            {
                                string endTagsString = string.Join("", openTags.Select(each => $"</{each.tagName}>").Reverse());
                                yield return new RichTextChunk
                                {
                                    IsIcon = false,
                                    Content = $"{curContent}{endTagsString}",
                                };
                            }

                            yield return new RichTextChunk
                            {
                                IsIcon = true,
                                Content = parsedResult.value,
                                // ReSharper disable once UseIndexFromEndExpression
                                IconColor = colors.Count > 0 ? colors[colors.Count - 1] : null,
                            };

                            // string textOpeningTags = string.Join("", openTags.Select(each => each.rawContent));
                            string textOpeningTags = string.Join("", openTags.Select(each => $"<{each.tagName}{(each.tagValueOrNull==null? "": $"={each.tagValueOrNull}")}>"));
                            richText = new StringBuilder(textOpeningTags);
                        }
                            break;
                        default:
                        {
                            // Debug.Log(parsedResult.content);
                            if (parsedResult.content != null && (parsedResult.content == "field" ||
                                                                 parsedResult.content.StartsWith("field.")))
                            {
                                (string error, int index, object value) fieldValue = Util.GetValue(property, fieldInfo, parent);
                                if (fieldValue.error != "")
                                {
#if SAINTSFIELD_DEBUG
                                    Debug.LogError(fieldValue.error);
#endif
                                }
                                else
                                {
                                    object finalValue = null;
                                    bool hasError = false;
                                    if(parsedResult.content == "field")
                                    {
                                        // richText.Append(RuntimeUtil.IsNull(fieldValue.value)
                                        //     ? ""
                                        //     : $"{fieldValue.value}");
                                        finalValue = fieldValue.value;
                                    }
                                    else
                                    {
                                        object accParent = fieldValue.value;
                                        // Debug.Log(parsedResult.content);
                                        (string error, int index, object value) accResult = ("Field value is null", -1, null);
                                        if(!RuntimeUtil.IsNull(accParent))
                                        {
                                            // ReSharper disable once ReplaceSubstringWithRangeIndexer
                                            string[] subFields = parsedResult.content.Substring("field.".Length).Split(SerializedUtils.pathSplitSeparator);
                                            foreach (string attrName in subFields)
                                            {
                                                MemberInfo accMemberInfo = null;
                                                foreach (var type in ReflectUtils.GetSelfAndBaseTypes(accParent))
                                                {
                                                    foreach (var info in type.GetMember(attrName,
                                                                 BindingFlags.Public | BindingFlags.NonPublic |
                                                                 BindingFlags.Instance | BindingFlags.Static |
                                                                 BindingFlags.FlattenHierarchy))
                                                    {
                                                        if (info == null) continue;
                                                        accMemberInfo = info;
                                                        break;
                                                    }
                                                }

                                                accResult = Util.GetValueAtIndex(-1, accMemberInfo, accParent);
                                                if (accResult.error != "")
                                                {
#if SAINTSFIELD_DEBUG
                                                    Debug.LogError($"{attrName} from {accParent}: {accResult.error}");
#endif
                                                    break;
                                                }

                                                accParent = accResult.value;
                                                if (accParent == null)
                                                {
                                                    accResult = ($"No target found for {attrName}", -1, null);
                                                    break;
                                                }
                                            }
                                        }

                                        if (accResult.error == "")
                                        {
                                            // Debug.Log($"{parsedResult.content}: {accResult.value}");
                                            // Debug.Log($"accResult.value={accResult.value}");
                                            finalValue = accResult.value;
                                        }
                                        else
                                        {
                                            hasError = true;
                                        }
                                        //
                                        // Debug.Log(string.Join(".", subFields));
                                    }

                                    if (!hasError)
                                    {
                                        string tagFinalResult;
                                        if (RuntimeUtil.IsNull(finalValue))
                                        {
                                            tagFinalResult = "";
                                        }
                                        else if (string.IsNullOrEmpty(parsedResult.value))
                                        {
                                            tagFinalResult = $"{finalValue}";
                                        }
                                        else
                                        {
                                            string formatString = $"{{0:{parsedResult.value}}}";
                                            try
                                            {
                                                tagFinalResult = string.Format(formatString, finalValue);
                                            }
#pragma warning disable CS0168
                                            catch (Exception ex)
#pragma warning restore CS0168
                                            {
#if SAINTSFIELD_DEBUG
                                                Debug.LogException(ex);
#endif
                                                tagFinalResult = $"{finalValue}";
                                            }
                                        }
                                        richText.Append(tagFinalResult);
                                    }
                                }
                            }
                            else
                            {
                                richText.Append(part);
                            }
                        }
                            break;
                    }
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
                if (endTagRawContent.Length > 0)
                {
                    return (RichPartType.EndTag, endTagRawContent.Trim(), null, false);
                }
                return (RichPartType.Content, part, null, false);
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
            // const string reg = @"(\w+)=(.+)";
            const string reg = @"([^\=]+)=(.+)";
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

            string tagNameStrip = tagName.Trim();

            if (tagNameStrip == "color")
            {
                tagValue = Colors.ToHtmlHexString(Colors.GetColorByStringPresent(tagValue));
                // Debug.Log(tagValue);
            }
            return (tagNameStrip, tagValue);
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
                            Util.LoadResource<Texture2D>(curChunk.Content),
                            Colors.GetColorByStringPresent(curChunk.IconColor),
                            -1,
                            Mathf.FloorToInt(position.height)
                        );
                        if (texture.width != 1 && texture.height != 1)
                        {
                            _textureCache.Add(cacheKey, texture);
                        }
                    }

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
                // GUI.Label(textRect, curGUIContent, textStyle);
                EditorGUI.LabelField(textRect, curGUIContent, textStyle);
                if (leftRect.width <= 0)
                {
                    return;
                }

                labelRect = leftRect;
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
                            unityTextAlign = TextAnchor.MiddleLeft,
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

                    if (!_textureCache.TryGetValue(cacheKey, out Texture texture))
                    {
                        texture = Util.LoadResource<Texture2D>(curChunk.Content);
                        if (texture.width != 1 && texture.height != 1)
                        {
                            _textureCache.Add(cacheKey, texture);
                        }
                    }

                    Image img = new Image
                    {
                        image = texture,
                        scaleMode = ScaleMode.ScaleToFit,
                        tintColor = Colors.GetColorByStringPresent(curChunk.IconColor),
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
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
                    img.style.flexShrink = 0;

#if EXT_INSPECTOR_LOG
                    Debug.Log($"#draw# icon <{curChunk.Content} {curChunk.IconColor}/>");
#endif
                    yield return img;
                }
            }
        }
#endif

        private Texture GetTexture2D(TextureCacheKey cacheKey, RichTextChunk curChunk, float height)
        {
            if (_textureCache.TryGetValue(cacheKey, out Texture texture))
            {
                return texture;
            }

            texture = Tex.TextureTo(
                Util.LoadResource<Texture2D>(curChunk.Content),
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
