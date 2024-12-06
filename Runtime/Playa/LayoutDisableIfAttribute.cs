using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutDisableIfAttribute: Attribute, IPlayaAttribute, ISaintsLayoutToggle
    {
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;
        public readonly EMode EditorMode;

        public LayoutDisableIfAttribute(params object[] by): this(EMode.Edit | EMode.Play, by)
        {
        }

        public LayoutDisableIfAttribute(EMode editorMode, params object[] by)
        {
            EditorMode = editorMode;
            ConditionInfos = Parser.Parse(by).ToArray();
        }
    }
}
