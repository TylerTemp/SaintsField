using System;
using System.Diagnostics;
#if SAINTSFIELD_IMPLICIT_USE
using JetBrains.Annotations;
#endif
using SaintsField.Utils;

namespace SaintsField.Playa
{
#if SAINTSFIELD_IMPLICIT_USE
    [MeansImplicitUse]  // https://github.com/TylerTemp/SaintsField/pull/171#issuecomment-3680042013
#endif
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
