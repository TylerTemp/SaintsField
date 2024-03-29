using System;
using System.Diagnostics;
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
        public readonly string[] Callbacks;
        public readonly EMode EditorMode;
        // ReSharper enable InconsistentNaming

        public ShowIfAttribute(EMode editorMode, params string[] andCallbacks)
        {
            EditorMode = editorMode;
            Callbacks = andCallbacks;
        }

        public ShowIfAttribute(params string[] andCallbacks): this(EMode.Edit | EMode.Play, andCallbacks)
        {
        }
    }
}
