using UnityEngine;

namespace SaintsField
{
    public class AdvancedDropdownAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string FuncName;

        public AdvancedDropdownAttribute(string funcName)
        {
            FuncName = funcName;
        }
    }
}
