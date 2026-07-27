using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.Renderer.ButtonFakeRenderer
{
    public partial class ButtonRenderer: AbsRenderer
    {
        private readonly SerializedObject _serializedObject;
        private readonly ButtonAttribute _buttonAttribute;

        private enum AsyncReturnType
        {
            None,
            Awaitable,
            UniTask,
        }

        public ButtonRenderer(ButtonAttribute buttonAttribute, SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _serializedObject = serializedObject;
            _buttonAttribute = buttonAttribute;
        }

        private static (AsyncReturnType returnAsync, Type returnAsyncValueType) GetAsyncReturnInfo(Type returnType)
        {
#if UNITY_6000_0_OR_NEWER
            if (typeof(UnityEngine.Awaitable).IsAssignableFrom(returnType))
            {
                return (AsyncReturnType.Awaitable, null);
            }

            foreach (Type genBaseType in ReflectUtils.GetGenBaseTypes(returnType))
            {
                if (genBaseType.GetGenericTypeDefinition() == typeof(UnityEngine.Awaitable<>))
                {
                    return (AsyncReturnType.Awaitable, genBaseType.GetGenericArguments()[0]);
                }
            }
#endif

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
            if (typeof(Cysharp.Threading.Tasks.UniTask).IsAssignableFrom(returnType))
            {
                return (AsyncReturnType.UniTask, null);
            }

            foreach (Type genBaseType in ReflectUtils.GetGenBaseTypes(returnType))
            {
                if (genBaseType.GetGenericTypeDefinition() == typeof(Cysharp.Threading.Tasks.UniTask<>))
                {
                    return (AsyncReturnType.UniTask, genBaseType.GetGenericArguments()[0]);
                }
            }
#endif

            return (AsyncReturnType.None, null);
        }


        private static object GetParameterDefaultValue(ParameterInfo parameterInfo)
        {
            if (parameterInfo.IsOptional)
            {
                return parameterInfo.DefaultValue;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(parameterInfo.ParameterType.IsValueType)
            {
                return Activator.CreateInstance(parameterInfo.ParameterType);
            }

            return null;
        }

        protected override bool AllowGuiColor => true;

#if UNITY_2021_3_OR_NEWER
        private readonly UnityEvent<string> _onSearchFieldUIToolkit = new UnityEvent<string>();
#endif

        public override void OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            _onSearchFieldUIToolkit.Invoke(searchString);
#endif
        }

        public override string ToString()
        {
            return $"<{FieldWithInfo.RenderType} {FieldWithInfo.MethodInfo?.Name}/>";
        }

        public override string GetField(string rawContent, string tagName, string tagValue)
        {
            return ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name);
        }
    }
}
