using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.ComponentHeader;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.HeaderGUI.Drawer
{
    public static class HeaderLabelDrawer
    {
        public static bool ValidateMethodInfo(MethodInfo methodInfo, Type reflectedType)
        {
            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                // ReSharper disable once InvertIf
                if (!parameterInfo.IsOptional)
                {
                    Debug.LogWarning($"method {methodInfo.Name}.{parameterInfo.Name} in {reflectedType} is not optional, skip");
                    return false;
                }
            }

            return true;
        }

        private static readonly Dictionary<string, IReadOnlyList<RichTextDrawer.RichTextChunk>> ParsedXmlCache = new Dictionary<string, IReadOnlyList<RichTextDrawer.RichTextChunk>>();

        private static string GetLabelName(object target, DrawHeaderGUI.RenderTargetInfo renderTargetInfo)
        {
            switch (renderTargetInfo.MemberType)
            {
                case DrawHeaderGUI.MemberType.Method:
                {
                    MethodInfo method = (MethodInfo)renderTargetInfo.MemberInfo;
                    return ObjectNames.NicifyVariableName(method.Name);
                }
                case DrawHeaderGUI.MemberType.Field:
                {
                    FieldInfo field = (FieldInfo)renderTargetInfo.MemberInfo;
                    return ObjectNames.NicifyVariableName(field.Name);
                }
                case DrawHeaderGUI.MemberType.Property:
                {
                    PropertyInfo property = (PropertyInfo)renderTargetInfo.MemberInfo;
                    return ObjectNames.NicifyVariableName(property.Name);
                }
                case DrawHeaderGUI.MemberType.Class:
                {
                    return target.GetType().Name;
                }
                default:
                    return "";
            }
        }

        public static (bool used, HeaderUsed headerUsed) Draw(object target, HeaderArea headerArea, HeaderLabelAttribute headerLabelAttribute, DrawHeaderGUI.RenderTargetInfo renderTargetInfo)
        {
            string rawLabel;
            string labelName = GetLabelName(target, renderTargetInfo);

            if (string.IsNullOrEmpty(headerLabelAttribute.Label))
            {
                switch (renderTargetInfo.MemberType)
                {
                    case DrawHeaderGUI.MemberType.Method:
                    {
                        MethodInfo method = (MethodInfo)renderTargetInfo.MemberInfo;
                        ParameterInfo[] methodParams = method.GetParameters();
                        object[] methodPass = new object[methodParams.Length];
                        // methodPass[0] = headerArea;
                        for (int index = 0; index < methodParams.Length; index++)
                        {
                            ParameterInfo param = methodParams[index];
                            object defaultValue = param.DefaultValue;
                            methodPass[index] = defaultValue;
                        }

                        object returnValue;
                        try
                        {
                            returnValue = method.Invoke(target, methodPass);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e.InnerException ?? e);
                            return (false, default);
                        }

                        rawLabel = GetStringFromResult(returnValue);

                    }
                        break;
                    case DrawHeaderGUI.MemberType.Field:
                    {
                        FieldInfo field = (FieldInfo)renderTargetInfo.MemberInfo;
                        object returnResult = field.GetValue(target);
                        rawLabel = GetStringFromResult(returnResult);

                    }
                        break;
                    case DrawHeaderGUI.MemberType.Property:
                    {
                        PropertyInfo property = (PropertyInfo)renderTargetInfo.MemberInfo;
                        object returnResult;
                        try
                        {
                            returnResult = property.GetValue(target);
                        }
                        catch (Exception e)
                        {
    #if SAINTSFIELD_DEBUG
                            Debug.LogException(e.InnerException ?? e);
    #endif
                            return (false, default);
                        }

                        rawLabel = GetStringFromResult(returnResult);

                    }
                        break;
                    case DrawHeaderGUI.MemberType.Class:
                    default:
                        return (false, default);
                }
            }
            else if(headerLabelAttribute.IsCallback)
            {
                (string error, object result) = Util.GetOf<object>(headerLabelAttribute.Label, null, null, renderTargetInfo.MemberInfo, target);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    return (false, default);
                }
                rawLabel = GetStringFromResult(result);
            }
            else
            {
                rawLabel = headerLabelAttribute.Label;
            }

            if (string.IsNullOrEmpty(rawLabel))
            {
                return (false, default);
            }

            if(rawLabel.Contains("<field") || !ParsedXmlCache.TryGetValue(rawLabel, out IReadOnlyList<RichTextDrawer.RichTextChunk> labelChunks))
            {
                ParsedXmlCache[rawLabel] = labelChunks =
                    RichTextDrawer.ParseRichXml(rawLabel, labelName, null, renderTargetInfo.MemberInfo, target).ToArray();
            }

            RichTextDrawer richTextDrawer = CacheAndUtil.GetCachedRichTextDrawer();
            GUIContent oldLabel = new GUIContent(labelName) { tooltip = headerLabelAttribute.Tooltip };
            float drawNeedWidth = richTextDrawer.GetWidth(oldLabel, headerArea.Height, labelChunks);

            float labelWidth = drawNeedWidth + 4;

            Rect usedRect = headerLabelAttribute.IsLeft
                ? headerArea.MakeXWidthRect(headerArea.GroupStartX, labelWidth)
                : headerArea.MakeXWidthRect(headerArea.GroupStartX - labelWidth, labelWidth);
            Rect labelRect = new Rect(usedRect)
            {
                x = usedRect.x + 2,
                width = usedRect.width - 4,
            };

            richTextDrawer.DrawChunks(labelRect, oldLabel, labelChunks);

            return (true, new HeaderUsed(usedRect));
        }

        private static string GetStringFromResult(object returnValue)
        {
            if (RuntimeUtil.IsNull(returnValue))
            {
                return null;
            }

            if (returnValue is string stringValue)
            {
                return stringValue;
            }

            return returnValue.ToString();
        }
    }
}
