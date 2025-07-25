using System;
using System.Collections.Generic;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TypeReferenceAttribute: PropertyAttribute
    {
        // public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        // public string GroupBy => "__LABEL_FIELD__";

        public readonly EType EType;
        public readonly IReadOnlyList<Type> SuperTypes;
        public readonly IReadOnlyList<string> OnlyAssemblies;
        public readonly IReadOnlyList<string> ExtraAssemblies;

        public TypeReferenceAttribute(EType eType = EType.Current, Type[] superTypes = null, string[] onlyAssemblies = null, string[] extraAssemblies = null)
        {
            EType = eType;
            SuperTypes = superTypes;
            OnlyAssemblies = onlyAssemblies;
            ExtraAssemblies = extraAssemblies;
        }
    }
}
