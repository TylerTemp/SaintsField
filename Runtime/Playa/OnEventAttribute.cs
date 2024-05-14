using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnEventAttribute: Attribute, IPlayaAttribute, IPlayaMethodAttribute, IPlayaMethodBindAttribute
    {
        public MethodBind MethodBind => MethodBind.CustomEvent;

        public string EventTarget { get; }
        public object Value { get; }
        public bool IsCallback { get; }

        public OnEventAttribute(string eventTarget, object value=null, bool isCallback=false)
        {
            EventTarget = eventTarget;
            Value = value;
            IsCallback = isCallback;
            if (isCallback)
            {
                Debug.Assert(value is string, $"{value} must be string when isCallback is true");
            }
        }
    }
}
