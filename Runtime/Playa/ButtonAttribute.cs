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

        public ButtonAttribute(string label = null)
        {
            (string content, bool isCallback) = RuntimeUtil.ParseCallback(label);
            Label = content;
            IsCallback = isCallback;
            // Debug.Log($"{IsCallback}/{content}");
        }
    }
}
