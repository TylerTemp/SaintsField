using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.ComponentHeader;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.HeaderGUI.Drawer
{
    public static class HeaderButtonDrawer
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



        private static readonly Dictionary<DrawHeaderGUI.RenderTargetInfo, IEnumerator> CachedCoroutines = new Dictionary<DrawHeaderGUI.RenderTargetInfo, IEnumerator>();

        public static (bool used, HeaderUsed headerUsed) Draw(object target, HeaderArea headerArea, HeaderButtonAttribute headerButtonAttribute, DrawHeaderGUI.RenderTargetInfo renderTargetInfo)
        {
            if (CachedCoroutines.TryGetValue(renderTargetInfo, out IEnumerator coroutine))
            {
                if (!coroutine.MoveNext())
                {
                    CachedCoroutines.Remove(renderTargetInfo);
                }
            }

            MethodInfo method = (MethodInfo)renderTargetInfo.MemberInfo;
            string friendlyName = ObjectNames.NicifyVariableName(method.Name);
            // string title;
            IReadOnlyList<RichTextDrawer.RichTextChunk> titleChunks;
            if (string.IsNullOrEmpty(headerButtonAttribute.Label))
            {
                titleChunks = new[]
                {
                    new RichTextDrawer.RichTextChunk
                    {
                        Content = friendlyName,
                        IconColor =  null,
                        IsIcon = false,
                        RawContent = friendlyName,
                    },
                };
            }
            else
            {
                string rawTitle = headerButtonAttribute.Label;

                if (headerButtonAttribute.IsCallback)
                {
                    (string error, string result) = Util.GetOf<string>(rawTitle, null,
                        null, method, target);
                    if (error != "")
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogError(error);
#endif
                        return (false, default);
                    }

                    if (string.IsNullOrEmpty(result))
                    {
                        return (false, default);
                    }

                    rawTitle = result;
                }

                if(rawTitle.Contains("<field") || !CacheAndUtil.ParsedXmlCache.TryGetValue(rawTitle, out titleChunks))
                {
                    // RichTextDrawer richTextDrawer = CacheAndUtil.GetCachedRichTextDrawer();
                    CacheAndUtil.ParsedXmlCache[rawTitle] = titleChunks =
                        RichTextDrawer.ParseRichXml(rawTitle, method.Name, null, method, target).ToArray();
                }
            }
            GUIContent oldLabel = new GUIContent(friendlyName);

            RichTextDrawer richTextDrawer = CacheAndUtil.GetCachedRichTextDrawer();

            float drawNeedWidth = richTextDrawer.GetWidth(oldLabel, headerArea.Height, titleChunks);


            // GUIContent content = new GUIContent(title);
            // Vector2 size = GUI.skin.button.CalcSize(content);
            // float buttonWidth = size.x;
            Rect usedRect = headerButtonAttribute.IsLeft
                ? headerArea.MakeXWidthRect(headerArea.GroupStartX, drawNeedWidth + 8)
                : headerArea.MakeXWidthRect(headerArea.GroupStartX - drawNeedWidth  - 8, drawNeedWidth + 8);
            Rect buttonRect = new Rect(usedRect)
            {
                x = usedRect.x + 2,
                width = usedRect.width - 4,
            };
            Rect labelRect = new Rect(buttonRect)
            {
                x = buttonRect.x + 2,
                width = buttonRect.width - 4,
            };

            GUIContent buttonContent = string.IsNullOrEmpty(headerButtonAttribute.Tooltip)
                ? GUIContent.none
                : new GUIContent("", headerButtonAttribute.Tooltip);

            GUIStyle style =
                headerButtonAttribute.IsGhost
                    ? CacheAndUtil.GetIconButtonStyle()
                    : EditorStyles.miniButton
                ;

            // ReSharper disable once InvertIf
            if (GUI.Button(buttonRect, buttonContent, style))
            {
                ParameterInfo[] methodParams = method.GetParameters();
                object[] methodPass = new object[methodParams.Length];
                // methodPass[0] = headerArea;
                for (int index = 0; index < methodParams.Length; index++)
                {
                    ParameterInfo param = methodParams[index];
                    object defaultValue = param.DefaultValue;
                    methodPass[index] = defaultValue;
                }

                // HeaderUsed methodReturn = default;
                object methodReturn = null;
                try
                {
                    methodReturn = method.Invoke(target, methodPass);
                }
                catch (Exception e)
                {
                    Debug.LogException(e.InnerException ?? e);
                }

                // Debug.Log($"methodReturn={methodReturn}");

                if (methodReturn is IEnumerator ie)
                {
                    CachedCoroutines[renderTargetInfo] = ie;
                }
            }

            richTextDrawer.DrawChunks(labelRect, oldLabel, titleChunks);

            return (true, new HeaderUsed(usedRect));
        }

        public static void Update()
        {
            List<DrawHeaderGUI.RenderTargetInfo> deleteKeys =
                new List<DrawHeaderGUI.RenderTargetInfo>(CachedCoroutines.Count);
            foreach (KeyValuePair<DrawHeaderGUI.RenderTargetInfo, IEnumerator> kv in CachedCoroutines)
            {
                if (!kv.Value.MoveNext())
                {
                    deleteKeys.Add(kv.Key);
                }
            }

            foreach (DrawHeaderGUI.RenderTargetInfo deleteKey in deleteKeys)
            {
                CachedCoroutines.Remove(deleteKey);
            }
        }
    }
}
