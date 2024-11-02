using System;
using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class AdvancedDropdownAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string FuncName;

        public const float DefaultTitleHeight = 45f;
        private const float DefaultSepHeight = 4f;

        public readonly float TitleHeight = DefaultTitleHeight;
        public readonly float ItemHeight = -1f;
        public readonly float SepHeight = DefaultSepHeight;
        public readonly float MinHeight = -1f;
        public readonly bool UseTotalItemCount = false;
        public readonly EUnique EUnique;

        // public AdvancedDropdownAttribute(string funcName)
        // {
        //     FuncName = funcName;
        // }

        // [Obsolete]
        // public AdvancedDropdownAttribute(string funcName, float itemHeight=-1f, float titleHeight=DefaultTitleHeight, float sepHeight=DefaultSepHeight, bool useTotalItemCount=false, float minHeight=-1f)
        // {
        //     FuncName = funcName;
        //     ItemHeight = itemHeight;
        //     TitleHeight = titleHeight;
        //     SepHeight = sepHeight;
        //     UseTotalItemCount = useTotalItemCount;
        //     MinHeight = minHeight;
        // }

        public AdvancedDropdownAttribute(string funcName = null, EUnique unique = EUnique.None)
        {
            FuncName = RuntimeUtil.ParseCallback(funcName, false).content;
            EUnique = unique;
        }

        public AdvancedDropdownAttribute(EUnique unique) : this(null, unique) {}
    }
}
