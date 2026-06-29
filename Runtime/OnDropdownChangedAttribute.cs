using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;
using TMPro;


namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnDropdownChangedAttribute: Attribute, IPlayaAttribute, IPlayaMethodAttribute, IPlayaMethodBindAttribute, IPlayaAutoRunnerFix
    {
        public MethodBind MethodBind => MethodBind.ComponentTypeAndName;

        public string EventTarget { get; }
        public Type ComponentTypeOrNull => typeof(TMP_Dropdown);
        public string ComponentEventName => nameof(TMP_Dropdown.onValueChanged);
        public object Value { get; }
        public bool IsCallback { get; }

        public OnDropdownChangedAttribute(string eventTarget=null, object value=null, bool isCallback=false)
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
