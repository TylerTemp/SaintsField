using System.Linq;
using System.Reflection;
using SaintsField.Unsaintly;
using UnityEditor;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Unsaintly.Renderer
{
    public class MethodRenderer: AbsRenderer
    {
        public MethodRenderer(UnityEditor.Editor editor, UnsaintlyFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(editor, fieldWithInfo, tryFixUIToolkit)
        {
        }

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            Object target = serializedObject.targetObject;
            MethodInfo methodInfo = fieldWithInfo.methodInfo;
            Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));
            ButtonAttribute buttonAttribute = (ButtonAttribute)methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Label;
            object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();

            return new Button(() =>
            {
                methodInfo.Invoke(target, defaultParams);
            })
            {
                text = buttonText,
                enableRichText = true,
            };
        }
#endif
        public override void Render()
        {
            Object target = serializedObject.targetObject;
            MethodInfo methodInfo = fieldWithInfo.methodInfo;

            Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));


            ButtonAttribute buttonAttribute = (ButtonAttribute)methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Label;

            // bool methodIsCoroutine = methodInfo.ReturnType == typeof(IEnumerator);
            // if (methodIsCoroutine)
            // {
            //     buttonEnabled &= (Application.isPlaying ? true : false);
            // }
            //
            // EditorGUI.BeginDisabledGroup(!buttonEnabled);

            if (GUILayout.Button(buttonText, new GUIStyle(GUI.skin.button) { richText = true }))
            {
                object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                methodInfo.Invoke(target, defaultParams);
                // IEnumerator methodResult = methodInfo.Invoke(target, defaultParams) as IEnumerator;
                //
                // if (!Application.isPlaying)
                // {
                //     // Set target object and scene dirty to serialize changes to disk
                //     EditorUtility.SetDirty(target);
                //
                //     PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                //     if (stage != null)
                //     {
                //         // Prefab mode
                //         EditorSceneManager.MarkSceneDirty(stage.scene);
                //     }
                //     else
                //     {
                //         // Normal scene
                //         EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                //     }
                // }
                // else if (methodResult != null && target is MonoBehaviour behaviour)
                // {
                //     behaviour.StartCoroutine(methodResult);
                // }
            }
        }

    }
}
