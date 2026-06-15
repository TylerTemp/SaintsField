using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer: SerializedFieldBaseRenderer, IMakeRenderer, IDOTweenPlayRecorder
    {
        private enum ParamType
        {
            TargetAndIndex,
            Target,
            Index,
        }

        private static (MethodInfo, ParamType) GetSearchMethodInfo(Type targetType, Type elementType, string methodName)
        {
            foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypesFromType(targetType))
            {
                foreach (MethodInfo methodInfo in eachType.GetMethods(ReflectUtils.FindTargetBindAttr))
                {
                    if (methodInfo.Name != methodName)
                    {
                        continue;
                    }

                    if (methodInfo.ReturnParameter?.ParameterType != typeof(bool))
                    {
                        continue;
                    }

                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    bool lastMatch = typeof(IEnumerable<ListSearchToken>).IsAssignableFrom(methodParams[methodParams.Length - 1].ParameterType);
                    if (!lastMatch)
                    {
                        continue;
                    }

                    if (methodParams.Length == 3
                        && elementType.IsAssignableFrom(methodParams[0].ParameterType)
                        && typeof(int).IsAssignableFrom(methodParams[1].ParameterType))
                    {
                        return (methodInfo, ParamType.TargetAndIndex);
                    }

                    if (methodParams.Length == 2 && elementType.IsAssignableFrom(methodParams[0].ParameterType))
                    {
                        return (methodInfo, ParamType.Target);
                    }

                    if (methodParams.Length == 2 && typeof(int).IsAssignableFrom(methodParams[0].ParameterType))
                    {
                        return (methodInfo, ParamType.Index);
                    }
                }
            }

            return (null, default);
        }

        private class AsyncSearchItems
        {
            public bool Started;
            public bool Finished;
            public IEnumerator<IReadOnlyList<int>> SourceGenerator;
            public List<int> FullSources;
            public string SearchText;
            public double DebounceSearchTime;
            public List<int> CachedFullSources;

            public List<int> ItemIndexToPropertyIndex;
            public int CurPageIndex;
        }

        private AsyncSearchItems _asyncSearchItems;

        protected override bool AllowGuiColor => false;
        private bool _arraySizeCondition;
        private bool _richLabelCondition;
        private bool _tableCondition;

        public ListDrawerSettingsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        public IEnumerable<IReadOnlyList<AbsRenderer>> MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
        }
    }
}
