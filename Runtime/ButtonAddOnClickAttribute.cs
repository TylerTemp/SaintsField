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

        public ButtonAddOnClickAttribute(string funcName, string buttonComp=null)
        {
            FuncName = funcName;
            ButtonComp = buttonComp;
        }
    }
}
