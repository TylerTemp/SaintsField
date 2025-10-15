using System;
using System.Diagnostics;
using SaintsField.Playa;
using SaintsField.Utils;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class BelowTextAttribute: Attribute, IPlayaAttribute, IPlayaClassAttribute
    {
        public readonly string Content;
        public readonly bool IsCallback;

        public bool Below = true;

        public BelowTextAttribute(string content)
        {
            (string contentParsed, bool isCallbackParsed) = RuntimeUtil.ParseCallback(content);

            Content = contentParsed;
            IsCallback = isCallbackParsed;
        }
    }
}
