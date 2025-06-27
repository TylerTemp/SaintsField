using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SaintsField.Utils
{
    public static class RuntimeUtil
    {
        public static (string content, bool isCallback) ParseCallback(string content, bool isCallback=false)
        {
            if (isCallback || content is null)
            {
                return (content, isCallback);
            }

            if (content.StartsWith("\\"))
            {
                return (content.Substring(1, content.Length - 1), false);
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (content.StartsWith("$"))
            {
                return (content.Substring(1, content.Length - 1), true);
            }
            // // ReSharper disable once ConvertIfStatementToReturnStatement
            // if (content.StartsWith(":"))
            // {
            //     return (content, true);
            // }

            return (content, false);
        }

        public static bool IsNull(object obj)
        {
            if (obj is Object uObject)
            {
                return !uObject;
            }

            return obj == null;
        }

        public static IEnumerable<string> SeperatePath(string path)
        {
            List<RichTextParsedChunk> openTag = new List<RichTextParsedChunk>();
            List<RichTextParsedChunk> acc = new List<RichTextParsedChunk>();
            foreach (RichTextParsedChunk richTextParsedChunk in ParseRichXml(path))
            {

                // Debug.Log($"richTextParsedChunk={richTextParsedChunk}");

                if (richTextParsedChunk.ChunkType == ChunkType.NormalTag)
                {
                    if (richTextParsedChunk.TagType == TagType.StartTag)
                    {
                        openTag.Add(richTextParsedChunk);
                    }
                    else
                    {
                        // ReSharper disable once UseIndexFromEndExpression
                        if (openTag.Count > 0 && openTag[openTag.Count - 1].TagName == richTextParsedChunk.TagName)
                        {
                            openTag.RemoveAt(openTag.Count - 1);
                        }
                    }

                    // Debug.Log($"add tag {richTextParsedChunk}");

                    acc.Add(richTextParsedChunk);
                }
                else if (richTextParsedChunk.ChunkType == ChunkType.IconTag)
                {
                    // Debug.Log($"add icon {richTextParsedChunk}");
                    acc.Add(richTextParsedChunk);
                }
                else if (richTextParsedChunk.ChunkType == ChunkType.Text)
                {
                    // acc.Add(richTextParsedChunk);
                    string rawContent = richTextParsedChunk.RawContent;
                    // Debug.Log($"rawContent={rawContent}");
                    // Debug.Log($"split={string.Join(":", split)}");
                    string[] split = rawContent.Split(new[] { '/' });
                    if (split.Length == 1)
                    {
                        acc.Add(richTextParsedChunk);
                    }
                    else
                    {
                        for (int index = 0; index < split.Length - 1; index++)
                        {
                            string splitSeg = split[index];

                            StringBuilder sb = new StringBuilder();

                            if (index != 0)
                            {
                                acc.AddRange(openTag);
                            }

                            if (acc.Count > 0)
                            {
                                foreach (RichTextParsedChunk accChunk in acc)
                                {
                                    // Debug.Log($"sb add={accChunk.RawContent}");
                                    sb.Append(accChunk.RawContent);
                                }
                                acc.Clear();
                            }

                            // Debug.Log($"splitSeg={splitSeg}");
                            sb.Append(splitSeg);

                            if (openTag.Count > 0)
                            {
                                foreach (RichTextParsedChunk openTagChunk in openTag)
                                {
                                    // Debug.Log($"sb add close tag={openTagChunk.TagName}");
                                    sb.Append($"</{openTagChunk.TagName}>");
                                }
                            }

                            // Debug.Log($"sb={sb}");
                            yield return sb.ToString();
                        }

                        acc.Clear();
                        acc.AddRange(openTag);

                        // ReSharper disable once UseIndexFromEndExpression
                        string lastStr = split[split.Length - 1];
                        // ReSharper disable once InvertIf
                        if(!string.IsNullOrEmpty(lastStr))
                        {
                            // Debug.Log($"add last text={lastStr}");
                            acc.Add(new RichTextParsedChunk(lastStr, ChunkType.Text));
                        }
                    }

                }
            }

            if (acc.Count > 0)
            {
                if (openTag.Count > 0)
                {
                    acc.AddRange(openTag);
                }

                yield return string.Join("", acc.Select(each => each.RawContent));
            }
        }

        private enum RichPartType
        {
            Content,
            StartTag,
            EndTag,
        }

        public enum ChunkType
        {
            Text,
            NormalTag,
            IconTag,
        }

        public enum TagType
        {
            None,
            StartTag,
            EndTag,
        }

        public readonly struct RichTextParsedChunk
        {
            public readonly string RawContent;

            public readonly ChunkType ChunkType;

            public readonly string TagName;
            public readonly TagType TagType;
            public readonly string TagValue;

            public readonly string IconColor;

            public RichTextParsedChunk(string rawContent, ChunkType chunkType, TagType tagType=TagType.None, string tagName="", string tagValue="", string iconColor="")
            {
                RawContent = rawContent;
                ChunkType = chunkType;
                TagName = tagName;
                TagType = tagType;
                TagValue = tagValue;
                IconColor = iconColor;
            }

            public override string ToString()
            {
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (ChunkType)
                {
                    case ChunkType.Text:
                        return $"[TEXT = {RawContent}]";
                    case ChunkType.IconTag:
                        return $"[ICON = {TagValue} {IconColor}]";
                    case ChunkType.NormalTag:
                        return $"[{(TagType == TagType.EndTag ? "/" : "")}TAG = {TagName} {TagValue}]";
                    default:
                        return base.ToString();
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<RichTextParsedChunk> ParseRichXml(string richXml)
        {
            List<string> colors = new List<string>();

            // Define a regular expression pattern to match the tags
            // const string pattern = "(<[^>]+>)";
            //
            // // Use Regex.Split to split the string by tags
            // string[] splitByTags = Regex.Split(richXml, pattern);
            string[] splitByTags = SplitByTags(richXml).Select(each => each.stringChunk).ToArray();

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
                    string curContent = richText.ToString();
                    if (curContent != "")
                    {
                        yield return new RichTextParsedChunk(curContent, ChunkType.Text);
                    }
                    richText = new StringBuilder();

                    // Debug.Log($"parse={parsedResult.content}, {parsedResult.value}");
                    openTags.Add((parsedResult.content, parsedResult.value, part));

                    // ReSharper disable once MergeIntoPattern
                    if (parsedResult.content == "color" && parsedResult.value != null)
                    {
                        colors.Add(parsedResult.value);
                    }

                    yield return new RichTextParsedChunk(part, ChunkType.NormalTag, tagType: TagType.StartTag,
                        parsedResult.content, parsedResult.value);
                }
                else if (parsedResult.partType == RichPartType.EndTag)
                {
                    if (!parsedResult.isSelfClose)
                    {
                        // ReSharper disable once UseIndexFromEndExpression
#if SAINTSFIELD_DEBUG
                        Debug.Assert(openTags[openTags.Count - 1].tagName == parsedResult.content,
                            parsedResult.content);
#endif
                        if (openTags.Count > 0)
                        {
                            openTags.RemoveAt(openTags.Count - 1);
                        }
                    }

                    if (parsedResult.content == "icon")
                    {
                        Debug.Assert(parsedResult.value != null);
                        // process ending
                        string curContent = richText.ToString();
                        if (curContent != "")
                        {
                            yield return new RichTextParsedChunk(curContent, ChunkType.Text);
                        }
                        richText = new StringBuilder();

                        for (int revIndex = openTags.Count - 1; revIndex <= 0; revIndex++)
                        {
                            (string tagName, string tagValueOrNull, string rawContent) closeTag = openTags[revIndex];
                            yield return new RichTextParsedChunk($"</{closeTag}>", ChunkType.IconTag,
                                tagType: TagType.EndTag, tagName: closeTag.tagName, tagValue: closeTag.tagValueOrNull);
                        }

                        yield return new RichTextParsedChunk(part,
                            // ReSharper disable once UseIndexFromEndExpression
                            ChunkType.IconTag, iconColor: colors.Count > 0 ? colors[colors.Count - 1] : null);

                        foreach ((string tagName, string tagValueOrNull, string rawContent) reOpenTag in openTags)
                        {
                            yield return new RichTextParsedChunk(reOpenTag.rawContent, ChunkType.NormalTag,
                                tagType: TagType.StartTag, tagName: reOpenTag.tagName,
                                tagValue: reOpenTag.tagValueOrNull);
                        }
                    }
                    else
                    {
                        if (parsedResult.content == "color")
                        {
                            if (colors.Count == 0)
                            {
#if SAINTSFIELD_DEBUG
                                Debug.LogError($"missing open color tag for {richText}");
#endif
                            }
                            else
                            {
                                colors.RemoveAt(colors.Count - 1);
                            }
                        }

                        string curContent = richText.ToString();
                        if (curContent != "")
                        {
                            yield return new RichTextParsedChunk(curContent, ChunkType.Text);
                        }

                        richText = new StringBuilder();

                        yield return new RichTextParsedChunk(part, ChunkType.NormalTag,
                            tagType: TagType.EndTag, tagName: parsedResult.content);
                    }
                }
                else
                {
                    richText.Append(part);
                }
            }

            string leftContent = richText.ToString();

            // ReSharper disable once InvertIf
            if (leftContent != "")
            {
                yield return new RichTextParsedChunk(leftContent, ChunkType.Text);
            }
        }

        public static IEnumerable<(bool isTag, string stringChunk)> SplitByTags(string richXml)
        {
            bool insideTag = false;
            bool insideDoubleQuote = false;

            StringBuilder contentBuilder = new StringBuilder();
            StringBuilder tagBuilder = new StringBuilder();


            foreach (char c in richXml)
            {
                if (c == '<')
                {
                    if (insideTag)
                    {
                        if (insideDoubleQuote)
                        {
                            tagBuilder.Append("<");
                        }
                        else  // abnormal inside tag, treat as content
                        {
                            string tagString = tagBuilder.ToString();
                            if (!string.IsNullOrEmpty(tagString))
                            {
                                yield return (false, tagString);
                                tagBuilder.Clear();
                            }
                            tagBuilder.Append("<");
                        }
                    }
                    else
                    {
                        string contentString = contentBuilder.ToString();
                        if (!string.IsNullOrEmpty(contentString))
                        {
                            yield return (false, contentString);
                            contentBuilder.Clear();
                        }
                        string tagString = tagBuilder.ToString();
                        if (!string.IsNullOrEmpty(tagString))
                        {
                            yield return (false, tagString);
                            tagBuilder.Clear();
                        }

                        insideTag = true;
                        tagBuilder.Append("<");
                    }
                }
                else if (c == '"')
                {
                    insideDoubleQuote = !insideDoubleQuote;
                    if (insideTag)
                    {
                        tagBuilder.Append("\"");
                    }
                    else
                    {
                        contentBuilder.Append("\"");
                    }
                }
                else if (c == '>')
                {
                    if (insideDoubleQuote)
                    {
                        if (insideTag)
                        {
                            tagBuilder.Append(c);
                        }
                        else
                        {
                            contentBuilder.Append(c);
                        }
                    }
                    else
                    {
                        if (insideTag)
                        {
                            tagBuilder.Append(">");
                            yield return (true, tagBuilder.ToString());
                            tagBuilder.Clear();
                            insideTag = false;
                        }
                        else
                        {
                            contentBuilder.Append(">");
                        }
                    }
                }
                else
                {
                    if (insideTag)
                    {
                        tagBuilder.Append(c);
                    }
                    else
                    {
                        contentBuilder.Append(c);
                    }
                }
            }


            string contentStringFinal = contentBuilder.ToString();
            if (!string.IsNullOrEmpty(contentStringFinal))
            {
                yield return (false, contentStringFinal);
                contentBuilder.Clear();
            }

            if(insideTag)  // abnormal insiding tag
            {
                string tagString = tagBuilder.ToString();
                if (!string.IsNullOrEmpty(tagString))
                {
                    yield return (false, tagString);
                    tagBuilder.Clear();
                }
            }
            // const string pattern = "(<[^>]+>)";
            //
            // // Use Regex.Split to split the string by tags
            // string[] splitByTags = Regex.Split(richXml, pattern);
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
    }
}
