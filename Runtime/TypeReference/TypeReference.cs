using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Serializable]
    public class TypeReference : ISerializationCallbackReceiver
    {
        [SerializeField] private string _typeNameAndAssembly;
        [SerializeField] private string _monoScriptGuid;

        private Type _type;

        public TypeReference()
        {
            _type = null;
            _monoScriptGuid = string.Empty;
        }

        public TypeReference(Type type)
        {
            _type = type;
            _monoScriptGuid =
#if UNITY_EDITOR
                EditorGetMonoGuid(type)
#else
                string.Empty
#endif
            ;
        }

        public Type Type
        {
            get
            {
                if (_type == null && !string.IsNullOrEmpty(_typeNameAndAssembly))
                {
                    _type = Type.GetType(_typeNameAndAssembly);
                }
                return _type;
            }
            set
            {
                // ReSharper disable once MergeIntoPattern
                if (value != null && value.FullName == null)
                {
                    throw new ArgumentException($"'{value}' does not have full name", nameof(value));
                }

                if (_type == value)
                {
                    return;
                }

                _type = value;
                _typeNameAndAssembly = GetTypeNameAndAssembly(value);
#if UNITY_EDITOR
                _monoScriptGuid = EditorGetMonoGuid(value);
#endif
            }
        }

        public static implicit operator Type(TypeReference typeReference) => typeReference?.Type;

        public static implicit operator TypeReference(Type type) => new TypeReference(type);

        public static string GetTypeNameAndAssembly(Type type)
        {
            return type != null
                ? $"{type.FullName}, {GetShortAssemblyName(type)}"
                : string.Empty;
        }

        public static string GetShortAssemblyName(Type type)
        {
            return GetShortAssemblyName(type.Assembly);
        }

        public static string GetShortAssemblyName(Assembly assembly)
        {
            string assemblyFullName = assembly.FullName;
            int commaIndex = assemblyFullName.IndexOf(',');
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            return assemblyFullName.Substring(0, commaIndex);
        }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // ReSharper disable once InvertIf
            if (!string.IsNullOrEmpty(_monoScriptGuid))
            {
                Type loadedType = Type.GetType(_typeNameAndAssembly);
                Type scriptType;
                try
                {
                    scriptType = EditorGetMonoType(_monoScriptGuid);
                }
                catch (UnityException)
                {
                    return;
                }

                if (scriptType != null && loadedType != scriptType)
                {
                    _typeNameAndAssembly = GetTypeNameAndAssembly(_type);
                }
            }
#endif
        }

#if UNITY_EDITOR
        public static Type EditorGetMonoType(string guid)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            return script == null
                ? null
                : script.GetClass();
        }
#endif

        public void OnAfterDeserialize()
        {
            _type = string.IsNullOrEmpty(_typeNameAndAssembly) ? null: Type.GetType(_typeNameAndAssembly);

        }

#if UNITY_EDITOR
        public static string EditorGetMonoGuid(Type type)
        {
            Type checkType = type.IsGenericType
                ? type.GetGenericTypeDefinition()
                : type;

            string typeName = type.Name;
            if (typeName.Contains("`"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                typeName = typeName.Substring(0, typeName.IndexOf('`'));
            }

            string foundGuid = string.Empty;

            foreach (string guid in AssetDatabase.FindAssets($"t:MonoScript {typeName}"))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                // ReSharper disable once UseNullPropagation
                if (monoScript is null)
                {
                    continue;
                }

                Type monoScriptType = monoScript.GetClass();
                // ReSharper disable once InvertIf
                if (monoScriptType == checkType)
                {
                    foundGuid = guid;
                    break;
                }
            }

            return foundGuid;
        }
#endif
    }
}
