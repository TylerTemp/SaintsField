using System;
using System.Diagnostics;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class TableHeadersAttribute: PropertyAttribute, IPlayaAttribute
    {
        // public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        // public string GroupBy => "";

        public readonly struct Header
        {
            public readonly string Name;
            public readonly bool IsCallback;

            public Header(string name)
            {
                (Name, IsCallback) = RuntimeUtil.ParseCallback(name);
            }
        }

        public readonly Header[] Headers;
        public virtual bool IsHide => false;

        public TableHeadersAttribute(params string[] headers)
        {
            Headers = new Header[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                Headers[i] = new Header(headers[i]);
            }
        }
    }
}
