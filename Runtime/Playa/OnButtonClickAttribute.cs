using System;
using System.Diagnostics;
using SaintsField.Interfaces;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class OnButtonClickAttribute: Attribute, IPlayaAttribute, IPlayaMethodAttribute, IPlayaMethodBindAttribute, IPlayaAutoRunnerFix
    {
        public MethodBind MethodBind => MethodBind.ButtonOnClick;

        public string EventTarget { get; }
        public object Value { get; }
        public bool IsCallback { get; }

        public OnButtonClickAttribute(string buttonTarget=null, object value=null, bool isCallback=false)
        {
            EventTarget = buttonTarget;
            Value = value;
            IsCallback = isCallback;
            if (isCallback)
            {
                Debug.Assert(value is string, $"{value} must be string when isCallback is true");
            }
        }
    }
}
