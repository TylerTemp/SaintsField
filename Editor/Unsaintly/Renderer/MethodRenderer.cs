using System.Linq;
using System.Reflection;
using SaintsField.Unsaintly;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Unsaintly.Renderer
{
    public class MethodRenderer: AbsRenderer
    {
        public MethodRenderer(UnityEditor.Editor editor, UnsaintlyFieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
        {
        }

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
