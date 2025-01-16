using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class MethodRenderer
    {
        private object[] _imGuiParameterValues;

        protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
        {
            if(_imGuiEnumerator != null)
            {
                bool moveNext = _imGuiEnumerator.MoveNext();
                if (!moveNext)
                {
                    _imGuiEnumerator = null;
                }

                // Debug.Log($"moved {_imGuiEnumerator}");
            }

            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ButtonAttribute buttonAttribute = null;
            List<IPlayaMethodBindAttribute> methodBindAttributes = new List<IPlayaMethodBindAttribute>();

            foreach (IPlayaAttribute playaAttribute in FieldWithInfo.PlayaAttributes)
            {
                switch (playaAttribute)
                {
                    case ButtonAttribute button:
                        buttonAttribute = button;
                        break;
                    case IPlayaMethodBindAttribute methodBindAttribute:
                        methodBindAttributes.Add(methodBindAttribute);
                        break;
                }
            }

            foreach (IPlayaMethodBindAttribute playaMethodBindAttribute in methodBindAttributes)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                Debug.Log($"button click {playaMethodBindAttribute}");
#endif
                CheckMethodBind(playaMethodBindAttribute, FieldWithInfo);
            }

            if (buttonAttribute == null)
            {
                return;
            }

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label)
                ? ObjectNames.NicifyVariableName(methodInfo.Name)
                : buttonAttribute.Label;

            ParameterInfo[] parameters = methodInfo.GetParameters();

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_imGuiParameterValues == null)
            {
                _imGuiParameterValues = parameters.Select(GetParameterDefaultValue).ToArray();
            }

            if (parameters.Length > 0)
            {
                GUILayout.BeginVertical(GUI.skin.box);
            }

            object[] invokeParams = parameters.Select((p, index) =>
            {
                return _imGuiParameterValues[index] = FieldLayout(_imGuiParameterValues[index], ObjectNames.NicifyVariableName(p.Name), p.ParameterType, false);
            }).ToArray();

            bool clicked;
            try
            {
                clicked = GUILayout.Button(" ", new GUIStyle(GUI.skin.button) { richText = true },
                    GUILayout.ExpandWidth(true));
            }
            catch (Exception e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif
                if (parameters.Length > 0)
                {
                    GUILayout.EndVertical();
                }

                return;
            }

            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks = GetRichIMGUI(buttonAttribute, methodInfo);
            GUIContent oldLabel = new GUIContent(ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name));
            Rect lastRect = GUILayoutUtility.GetLastRect();
            float drawNeedWidth = _richTextDrawer.GetWidth(oldLabel, lastRect.height, richTextChunks);
            Rect drawRect = drawNeedWidth > lastRect.width
                ? lastRect
                // center it
                : new Rect(lastRect.x + (lastRect.width - drawNeedWidth) / 2, lastRect.y, drawNeedWidth, lastRect.height);
            _richTextDrawer.DrawChunks(drawRect, oldLabel, richTextChunks);

            if (parameters.Length > 0)
            {
                GUILayout.EndVertical();
            }

            // ReSharper disable once InvertIf
            if (clicked)
            {
                // Debug.Log($"Button Clicked: {buttonText}");
                object result = methodInfo.Invoke(target, invokeParams);
                // Debug.Log($"Button return: {result}/{result is IEnumerator}");
                if (result is IEnumerator ie)
                {
                    _imGuiEnumerator = ie;
                }
            }
        }
    }
}
