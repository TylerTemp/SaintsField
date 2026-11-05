using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LayoutHideIfAttribute: LayoutShowIfAttribute
    {
        public LayoutHideIfAttribute(params object[] by): base(0, by)
        {
        }

        public LayoutHideIfAttribute(EMode editorMode, params object[] by): base(editorMode, by)
        {
        }
    }
}
