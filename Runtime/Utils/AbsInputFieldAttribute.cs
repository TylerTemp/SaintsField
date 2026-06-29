using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;
using TMPro;

namespace SaintsField.Utils
{
    public abstract class AbsInputFieldAttribute: Attribute, IPlayaAttribute, IPlayaMethodAttribute, IPlayaMethodBindAttribute, IPlayaAutoRunnerFix
    {
        public MethodBind MethodBind => MethodBind.ComponentTypeAndName;

        public string EventTarget { get; }
        public Type ComponentTypeOrNull => typeof(TMP_InputField);
        public abstract string ComponentEventName { get; }
        public object Value { get; }
        public bool IsCallback { get; }

        public AbsInputFieldAttribute(string eventTarget=null, object value=null, bool isCallback=false)
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
