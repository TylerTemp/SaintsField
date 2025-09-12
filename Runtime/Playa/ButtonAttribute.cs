using System;
using System.Diagnostics;
using JetBrains.Annotations;
using SaintsField.Utils;

namespace SaintsField.Playa
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute: Attribute, IPlayaAttribute, IPlayaMethodAttribute
    {
        public readonly string Label;
        public readonly bool IsCallback;
        public readonly bool HideReturnValue;

        public ButtonAttribute(string label = null, bool hideReturnValue = false)
        {
            (string content, bool isCallback) = RuntimeUtil.ParseCallback(label);
            Label = content;
            IsCallback = isCallback;
            HideReturnValue = hideReturnValue;
            // Debug.Log($"{IsCallback}/{content}");
        }
    }
}
