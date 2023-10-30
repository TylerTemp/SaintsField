using UnityEngine;

namespace ExtInspector
{
    public class DropdownAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string FuncName;

        public DropdownAttribute(string funcName)
        {
            FuncName = funcName;
        }
    }
}
