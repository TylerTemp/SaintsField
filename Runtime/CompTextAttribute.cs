using System;
using System.Diagnostics;
using SaintsField.Playa;
using SaintsField.Utils;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class CompTextAttribute: Attribute, IPlayaAttribute
    {
        public readonly string Content;
        public readonly bool IsCallback;
        public readonly float PaddingLeft;
        public readonly float PaddingRight;

        public CompTextAttribute(string content, float paddingLeft=4, float paddingRight=0)
        {
            (string contentParsed, bool isCallbackParsed) = RuntimeUtil.ParseCallback(content);

            Content = contentParsed;
            IsCallback = isCallbackParsed;
            PaddingLeft = paddingLeft;
            PaddingRight = paddingRight;
        }
    }
}
