using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public abstract class DecButtonAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly string FuncName;
        public readonly string ButtonLabel;
        public readonly bool IsCallback;

        protected DecButtonAttribute(string funcName, string buttonLabel=null, bool isCallback=false, string groupBy = "")
        {
            FuncName = funcName;

            (string content, bool isCallback) parsed = RuntimeUtil.ParseCallback(buttonLabel, isCallback);
            ButtonLabel = parsed.content;
            IsCallback = parsed.isCallback;

            GroupBy = groupBy;
        }
    }
}
