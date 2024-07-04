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
    public class ShowIfAttribute: PropertyAttribute, ISaintsAttribute, IImGuiVisibilityAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Visibility;
        public string GroupBy => "";

        // ReSharper disable InconsistentNaming
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;
        public readonly EMode EditorMode;
        // ReSharper enable InconsistentNaming

        #region callback

        public ShowIfAttribute(EMode editorMode, params object[] andCallbacks)
        {
            EditorMode = editorMode;
            ConditionInfos = Parser.Parse(andCallbacks).ToArray();
        }

        public ShowIfAttribute(params object[] andCallbacks): this(EMode.Edit | EMode.Play, andCallbacks)
        {
        }
        #endregion
    }
}
