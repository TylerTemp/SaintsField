using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class DropdownAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string FuncName;
        public readonly bool SlashAsSub;

        public DropdownAttribute(string funcName, bool slashAsSub=true)
        {
            FuncName = funcName;
            SlashAsSub = slashAsSub;
        }
    }
}
