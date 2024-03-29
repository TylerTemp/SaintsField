using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class AdvancedDropdownAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string FuncName;

        private const float DefaultTitleHeight = 45f;
        private const float DefaultSepHeight = 4f;

        public readonly float TitleHeight;
        public readonly float ItemHeight;
        public readonly float SepHeight;
        public readonly float MinHeight;
        public readonly bool UseTotalItemCount;

        // public AdvancedDropdownAttribute(string funcName)
        // {
        //     FuncName = funcName;
        // }

        public AdvancedDropdownAttribute(string funcName, float itemHeight=-1f, float titleHeight=DefaultTitleHeight, float sepHeight=DefaultSepHeight, bool useTotalItemCount=false, float minHeight=-1f)
        {
            FuncName = funcName;
            ItemHeight = itemHeight;
            TitleHeight = titleHeight;
            SepHeight = sepHeight;
            UseTotalItemCount = useTotalItemCount;
            MinHeight = minHeight;
        }
    }
}
