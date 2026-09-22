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
    public abstract class PathedDropdownAttribute: PropertyAttribute, ISaintsAttribute, IPathedDropdownAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public string FuncName { get; set; }

        public EUnique EUnique { get; set; }
        public virtual PathedMode PathedMode => PathedMode.Default;

        public IReadOnlyList<object> Options { get; set; }
        public IReadOnlyList<(string path, object value)> Tuples { get; set; }

        // ReSharper disable once InconsistentNaming
        public bool slashAsSub { get; set; }

        protected PathedDropdownAttribute(string funcName = null, EUnique unique = EUnique.None, bool slashAsSub=true)
        {
            FuncName = RuntimeUtil.ParseCallback(funcName).content;
            EUnique = unique;
            this.slashAsSub = slashAsSub;
        }

        protected PathedDropdownAttribute(EUnique unique) : this(null, unique) {}
    }
}
