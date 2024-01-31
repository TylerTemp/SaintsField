using UnityEngine;

namespace SaintsField
{
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
            ButtonLabel = buttonLabel;
            IsCallback = isCallback;
            GroupBy = groupBy;
        }
    }
}
