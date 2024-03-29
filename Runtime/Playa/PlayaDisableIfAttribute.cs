using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class PlayaDisableIfAttribute: Attribute, IPlayaAttribute
    {
        // ReSharper disable InconsistentNaming
        public readonly string[] Callbacks;
        public readonly EMode EditorMode;
        // ReSharper enable InconsistentNaming

        public PlayaDisableIfAttribute(params string[] by): this(EMode.Edit | EMode.Play, by)
        {
        }

        public PlayaDisableIfAttribute(EMode editorMode, params string[] by)
        {
            EditorMode = editorMode;
            Callbacks = by;
        }
    }
}
