using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;


namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnToggleChangedAttribute: Attribute, IPlayaSaintsEditorOnlyAttribute, IPlayaMethodAttribute, IPlayaMethodBindAttribute, IPlayaAutoRunnerFix
    {
        public MethodBind MethodBind => MethodBind.ComponentTypeAndName;

        public string EventTarget { get; }
        public Type ComponentTypeOrNull => typeof(UnityEngine.UI.Toggle);
        public string ComponentEventName => nameof(UnityEngine.UI.Toggle.onValueChanged);
        public object Value { get; }
        public bool IsCallback { get; }

        public OnToggleChangedAttribute(string eventTarget=null, object value=null, bool isCallback=false)
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
