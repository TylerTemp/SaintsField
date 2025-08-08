using System;
using System.Collections.Generic;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public abstract class PathedDropdownAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string FuncName;

        public EUnique EUnique;

        public enum Mode
        {
            Default,
            Options,
            Tuples,
        }

        public virtual Mode BehaveMode => Mode.Default;

        public IReadOnlyList<object> Options;
        public IReadOnlyList<(string path, object value)> Tuples;

        protected PathedDropdownAttribute(string funcName = null, EUnique unique = EUnique.None)
        {
            FuncName = RuntimeUtil.ParseCallback(funcName).content;
            EUnique = unique;
        }

        protected PathedDropdownAttribute(EUnique unique) : this(null, unique) {}
    }
}
