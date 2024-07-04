using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class PlayaDisableIfAttribute: Attribute, IPlayaAttribute
    {
        // ReSharper disable InconsistentNaming
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;
        public readonly EMode EditorMode;
        // ReSharper enable InconsistentNaming

        public PlayaDisableIfAttribute(EMode editorMode, params object[] by)
        {
            EditorMode = editorMode;
            ConditionInfos = Parser.Parse(by).ToArray();
        }

        public PlayaDisableIfAttribute(params object[] by): this(EMode.Edit | EMode.Play, by)
        {
        }
    }
}
