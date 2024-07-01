using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class ButtonAddOnClickAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string FuncName;
        public readonly string ButtonComp;

        public readonly object Value;
        public readonly bool IsCallback;

        public ButtonAddOnClickAttribute(string funcName, string buttonComp=null, object value=null, bool isCallback=false)
        {
            FuncName = funcName;
            ButtonComp = buttonComp;
            Value = value;
            IsCallback = isCallback;
        }
    }
}
