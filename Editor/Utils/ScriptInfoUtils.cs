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
            // Debug.Log($"type {systemType} -> {ms}");
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

            if (typeof(SaintsEditorWindow).IsAssignableFrom(type))  // This does not fall into monoscript, even it's a scriptable object
            {
                string[] guids = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    MonoScript ms = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    // ReSharper disable once InvertIf
                    if (ms != null)
                    {
                        // Debug.Log(ms.name);
                        // Debug.Log(ms.GetClass());
                        // Debug.Log(ms.GetClass() == type);
                        if (ms.GetClass() == type)
                        {
                            return (true, ms);
                        }
                    }
                }
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
