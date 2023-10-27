using ExtInspector.Standalone;
using UnityEngine;

namespace ExtInspector
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class AboveButtonAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly string FuncName;
        public readonly string ButtonLabel;
        public readonly bool ButtonLabelIsCallback;

        public AboveButtonAttribute(string funcName, string buttonLabel, bool buttonLabelIsCallback=false, string groupBy = "")
        {
            FuncName = funcName;
            ButtonLabel = buttonLabel;
            ButtonLabelIsCallback = buttonLabelIsCallback;
            GroupBy = groupBy;
        }
    }
}
