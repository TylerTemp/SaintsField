using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        // ReSharper disable InconsistentNaming
        public readonly string[] ReadOnlyBys;
        public readonly EMode EditorMode;
        // ReSharper enable InconsistentNaming

        public ReadOnlyAttribute(params string[] by): this(EMode.Edit | EMode.Play, by)
        {
        }

        public ReadOnlyAttribute(EMode editorMode, params string[] by)
        {
            EditorMode = editorMode;

            ReadOnlyBys = by.Length == 0
                ? null
                : by;

            GroupBy = "";
        }
    }
}
