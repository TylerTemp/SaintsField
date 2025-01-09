using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if(_imGuiEnumerator != null && !_imGuiEnumerator.MoveNext())
            {
                _imGuiEnumerator = null;
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

            if (GUILayout.Button(buttonText, new GUIStyle(GUI.skin.button) { richText = true },
                    GUILayout.ExpandWidth(true)))
            {
                object result = methodInfo.Invoke(target, invokeParams);
                if (result is IEnumerator ie)
                {
                    _imGuiEnumerator = ie;
                }
            }

            if (parameters.Length > 0)
            {
                GUILayout.EndVertical();
            }
        }
    }
}
