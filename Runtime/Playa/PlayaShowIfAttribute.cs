using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class PlayaShowIfAttribute: Attribute, IPlayaAttribute
    {
        // ReSharper disable InconsistentNaming
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;
        public readonly EMode EditorMode;
        // ReSharper enable InconsistentNaming

        public PlayaShowIfAttribute(EMode editorMode, params object[] andCallbacks)
        {
            EditorMode = editorMode;
            ConditionInfos = Parser.Parse(andCallbacks).ToArray();
        }

        public PlayaShowIfAttribute(params object[] andCallbacks): this(EMode.Edit | EMode.Play, andCallbacks)
        {
        }
    }
}
