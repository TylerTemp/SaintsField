using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using SaintsField.Condition;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        // ReSharper disable InconsistentNaming
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;
        public readonly EMode EditorMode;
        // ReSharper enable InconsistentNaming

        public ReadOnlyAttribute(params object[] by): this(EMode.Edit | EMode.Play, by)
        {
        }

        public ReadOnlyAttribute(EMode editorMode, params object[] by)
        {
            EditorMode = editorMode;
            ConditionInfos = Parser.Parse(by).ToArray();
        }
    }
}
