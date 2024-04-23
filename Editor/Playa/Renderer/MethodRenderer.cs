using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class MethodRenderer: AbsRenderer
    {
        public MethodRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(serializedObject, fieldWithInfo, tryFixUIToolkit)
        {
            Debug.Assert(FieldWithInfo.MethodInfo.GetParameters().All(p => p.IsOptional), $"{FieldWithInfo.MethodInfo.Name} has non-optional parameters");
        }

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));
            ButtonAttribute[] buttonAttributes = methodInfo.GetCustomAttributes<ButtonAttribute>(true).ToArray();
            if (buttonAttributes.Length == 0)
            {
                return null;
            }

            ButtonAttribute buttonAttribute = buttonAttributes[0];

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Label;
            object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();

            Button result = new Button(() => methodInfo.Invoke(target, defaultParams))
            {
                text = buttonText,
                enableRichText = true,
                style =
                {
                    flexGrow = 1,
                },
            };
            if (FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute || each is PlayaHideIfAttribute || each is PlayaEnableIfAttribute ||
                                                            each is PlayaDisableIfAttribute) > 0)
            {
                result.RegisterCallback<AttachToPanelEvent>(_ => result.schedule.Execute(() => UIToolkitOnUpdate(FieldWithInfo, result, true)).Every(100));
            }

            return result;
        }
#endif
        public override void Render()
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ButtonAttribute[] buttonAttributes = methodInfo.GetCustomAttributes<ButtonAttribute>(true).ToArray();
            if (buttonAttributes.Length == 0)
            {
                return;
            }

            ButtonAttribute buttonAttribute = buttonAttributes[0];

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Label)
                    ? ObjectNames.NicifyVariableName(methodInfo.Name)
                    : buttonAttribute.Label;

                if (GUILayout.Button(buttonText, new GUIStyle(GUI.skin.button) { richText = true },
                        GUILayout.ExpandWidth(true)))
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    methodInfo.Invoke(target, defaultParams);
                }
            }
        }

        public override float GetHeight()
        {
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            if(methodInfo.GetCustomAttribute<ButtonAttribute>(true) == null)
            {
                return 0;
            }

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return 0;
            }

            return SaintsPropertyDrawer.SingleLineHeight;
        }

        public override void RenderPosition(Rect position)
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ButtonAttribute[] buttonAttributes = methodInfo.GetCustomAttributes<ButtonAttribute>(true).ToArray();
            if (buttonAttributes.Length == 0)
            {
                return;
            }

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {

                ButtonAttribute buttonAttribute = buttonAttributes[0];

                string buttonText = string.IsNullOrEmpty(buttonAttribute.Label)
                    ? ObjectNames.NicifyVariableName(methodInfo.Name)
                    : buttonAttribute.Label;

                if (GUI.Button(position, buttonText, new GUIStyle(GUI.skin.button) { richText = true }))
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    methodInfo.Invoke(target, defaultParams);
                }
            }
        }
    }
}
