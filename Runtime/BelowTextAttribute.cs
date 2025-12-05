using System;
using System.Diagnostics;
using SaintsField.Playa;
using SaintsField.Utils;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class BelowTextAttribute: Attribute, IPlayaAttribute, IPlayaClassAttribute
    {
        public readonly string Content;
        public readonly bool IsCallback;

        public readonly float PaddingLeft;
        public readonly float PaddingRight;

        public bool Below = true;

        public BelowTextAttribute(string content, float paddingLeft=0, float paddingRight=0)
        {
            (string contentParsed, bool isCallbackParsed) = RuntimeUtil.ParseCallback(content);

            Content = contentParsed;
            IsCallback = isCallbackParsed;

            PaddingLeft = paddingLeft;
            PaddingRight = paddingRight;
        }

        public virtual bool EndDecorator => true;
    }
}
