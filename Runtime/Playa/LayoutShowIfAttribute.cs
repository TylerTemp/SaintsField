using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutShowIfAttribute: Attribute, IPlayaAttribute, ISaintsLayoutToggle
    {
        public IReadOnlyList<ConditionInfo> ConditionInfos { get; }
        public EMode EditorMode { get; }

        public LayoutShowIfAttribute(EMode editorMode, params object[] by)
        {
            EditorMode = editorMode;
            ConditionInfos = Parser.Parse(by).ToArray();
        }

        public LayoutShowIfAttribute(params object[] by): this(EMode.Edit | EMode.Play, by)
        {
        }

        public override string ToString()
        {
            return $"<LayoutShowIfAttribute eMode={EditorMode} conditions={string.Join(", ", ConditionInfos)}>";
        }
    }
}
