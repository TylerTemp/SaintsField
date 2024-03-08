using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
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

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            object target = FieldWithInfo.target;
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

            return new Button(() =>
            {
                methodInfo.Invoke(target, defaultParams);
            })
            {
                text = buttonText,
                enableRichText = true,
                style =
                {
                    flexGrow = 1,
                },
            };
        }
#endif
        public override void Render()
        {
            object target = FieldWithInfo.target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ButtonAttribute[] buttonAttributes = methodInfo.GetCustomAttributes<ButtonAttribute>(true).ToArray();
            if (buttonAttributes.Length == 0)
            {
                return;
            }

            ButtonAttribute buttonAttribute = buttonAttributes[0];

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Label;

            if (GUILayout.Button(buttonText, new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.ExpandWidth(true)))
            {
                object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                methodInfo.Invoke(target, defaultParams);
            }
        }

        public override float GetHeight(SerializedProperty property)
        {
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            if(methodInfo.GetCustomAttribute<ButtonAttribute>(true) == null)
            {
                return 0;
            }
            return SaintsPropertyDrawer.SingleLineHeight;
        }

        public override void RenderPosition(Rect position, SerializedProperty property)
        {
            object target = FieldWithInfo.target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ButtonAttribute[] buttonAttributes = methodInfo.GetCustomAttributes<ButtonAttribute>(true).ToArray();
            if (buttonAttributes.Length == 0)
            {
                return;
            }

            ButtonAttribute buttonAttribute = buttonAttributes[0];

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Label;

            if (GUI.Button(position, buttonText, new GUIStyle(GUI.skin.button) { richText = true }))
            {
                object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                methodInfo.Invoke(target, defaultParams);
            }
        }
    }
}
