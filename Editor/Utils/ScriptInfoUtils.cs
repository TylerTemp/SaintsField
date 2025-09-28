using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class ScriptInfoUtils
    {
        private static Dictionary<Type, IReadOnlyList<CodeAnalysisUtils.MemberContainer>> _cache = new Dictionary<Type, IReadOnlyList<CodeAnalysisUtils.MemberContainer>>();

        public static IReadOnlyList<CodeAnalysisUtils.MemberContainer> GetMembersCorrectOrder(Type systemType)
        {
#if SAINTSFIELD_CODE_ANALYSIS
            if (_cache.TryGetValue(systemType, out IReadOnlyList<CodeAnalysisUtils.MemberContainer> cached))
            {
                return cached;
            }

            (bool found, MonoScript ms) = GetMonoScriptFromType(systemType);
            if (!found)
            {
                return Array.Empty<CodeAnalysisUtils.MemberContainer>();
            }

            foreach (CodeAnalysisUtils.ClassContainer container in CodeAnalysisUtils.Parse(ms))
            {
                if (container.Name == systemType.Name)
                {
                    return _cache[systemType] = container.Members;
                }
            }
#endif
            return Array.Empty<CodeAnalysisUtils.MemberContainer>();
        }

        public static (bool found, MonoScript script) GetMonoScriptFromType(Type type)
        {
            if(!typeof(Component).IsAssignableFrom(type) && !typeof(ScriptableObject).IsAssignableFrom(type))
            {
                // Debug.LogWarning($"Type {type} is not a Component or ScriptableObject, cannot find MonoScript.");
                return (false, null);
            }

            foreach (MonoScript runtimeMonoScript in MonoImporter.GetAllRuntimeMonoScripts())
            {
                if (runtimeMonoScript.GetClass() == type)
                {
                    // Debug.Log(runtimeMonoScript);
                    return (true, runtimeMonoScript);
                }
            }

            return (false, null);
        }
    }
}
