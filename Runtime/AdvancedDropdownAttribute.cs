using UnityEngine;

namespace SaintsField
{
    public class AdvancedDropdownAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string FuncName;

        private const float DefaultTitleHeight = 31f;

        public readonly float TitleHeight;
        public readonly float ItemHeight;
        public readonly float MinHeight;

        // public AdvancedDropdownAttribute(string funcName)
        // {
        //     FuncName = funcName;
        // }

        public AdvancedDropdownAttribute(string funcName, float itemHeight=-1f, float titleHeight=DefaultTitleHeight, float minHeight=-1f)
        {
            FuncName = funcName;
            ItemHeight = itemHeight;
            TitleHeight = titleHeight;
            MinHeight = minHeight;
        }
    }
}
