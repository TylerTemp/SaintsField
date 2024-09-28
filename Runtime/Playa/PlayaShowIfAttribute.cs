using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PlayaShowIfAttribute: Attribute, IPlayaAttribute, IVisibilityAttribute
    {
        public IReadOnlyList<ConditionInfo> ConditionInfos { get; }
        public EMode EditorMode { get; }
        public virtual bool IsShow => true;

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
