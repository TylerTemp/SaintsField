using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;


namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnDropdownChangedAttribute: Attribute, IPlayaAttribute, IPlayaMethodAttribute, IPlayaMethodBindAttribute, IPlayaAutoRunnerFix
    {
        public MethodBind MethodBind => MethodBind.TMPDropdownOnValueChanged;

        public string EventTarget { get; }
        public object Value { get; }
        public bool IsCallback { get; }

        public OnDropdownChangedAttribute(string eventTarget, object value=null, bool isCallback=false)
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
