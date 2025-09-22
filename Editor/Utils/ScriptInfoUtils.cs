using System;
using System.Collections.Generic;
using UnityEditor;

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

        private static (bool found, MonoScript script) GetMonoScriptFromType(Type type)
        {
            if (typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(type))
            {
                foreach (MonoScript runtimeMonoScript in MonoImporter.GetAllRuntimeMonoScripts())
                {
                    if (runtimeMonoScript.GetClass() == type)
                    {
                        return (true, runtimeMonoScript);
                    }
                }
            }
            else if(typeof(UnityEngine.ScriptableObject).IsAssignableFrom(type))
            {
                UnityEngine.ScriptableObject so = UnityEngine.ScriptableObject.CreateInstance(type);
                MonoScript ms = MonoScript.FromScriptableObject(so);
                UnityEngine.Object.DestroyImmediate(so);
                return (true, ms);
            }

            return (false, null);
        }


    }
}
