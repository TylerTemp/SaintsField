using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Condition;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ShowIfAttribute: PropertyAttribute, ISaintsAttribute, IVisibilityAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Visibility;
        public string GroupBy => "";

        public IReadOnlyList<ConditionInfo> ConditionInfos { get; }
        public EMode EditorMode { get; }
        public virtual bool IsShow => true;

        public ShowIfAttribute(EMode editorMode, params object[] andCallbacks)
        {
            EditorMode = editorMode;
            ConditionInfos = Parser.Parse(andCallbacks).ToArray();
        }

        public ShowIfAttribute(params object[] andCallbacks): this(EMode.Edit | EMode.Play, andCallbacks)
        {
        }
    }
}
