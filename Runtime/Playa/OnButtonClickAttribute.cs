using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class OnButtonClickAttribute: Attribute, IPlayaAttribute, IPlayaMethodAttribute, IPlayaMethodBindAttribute
    {
        public MethodBind MethodBind => MethodBind.ButtonOnClick;

        public string ButtonTarget { get; }
        public object Value { get; }
        public bool IsCallback { get; }

        public OnButtonClickAttribute(string buttonTarget=null, object value=null, bool isCallback=false)
        {
            ButtonTarget = buttonTarget;
            Value = value;
            IsCallback = isCallback;
            if (isCallback)
            {
                Debug.Assert(value is string, $"{value} must be string when isCallback is true");
            }
        }
    }
}
