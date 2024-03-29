using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class PlayaShowIfAttribute: Attribute, IPlayaAttribute
    {
        // ReSharper disable InconsistentNaming
        public readonly string[] Callbacks;
        public readonly EMode EditorMode;
        // ReSharper enable InconsistentNaming

        public PlayaShowIfAttribute(EMode editorMode, params string[] andCallbacks)
        {
            EditorMode = editorMode;
            Callbacks = andCallbacks;
        }

        public PlayaShowIfAttribute(params string[] andCallbacks): this(EMode.Edit | EMode.Play, andCallbacks)
        {
        }
    }
}
