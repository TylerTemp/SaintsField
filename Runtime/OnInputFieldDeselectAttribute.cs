using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.Utils;
using TMPro;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnInputFieldDeselectAttribute: AbsInputFieldAttribute
    {
        public override string ComponentEventName => nameof(TMP_InputField.onDeselect);

        public OnInputFieldDeselectAttribute(string eventTarget = null, object value = null, bool isCallback = false) : base(eventTarget, value, isCallback)
        {
        }
    }
}
