using UnityEngine;

namespace SaintsField
{
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
