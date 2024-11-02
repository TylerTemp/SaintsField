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
        public readonly EUnique EUnique;

        public DropdownAttribute(string funcName=null, bool slashAsSub=true, EUnique unique=EUnique.None)
        {
            FuncName = funcName;
            SlashAsSub = slashAsSub;
            EUnique = unique;
        }

        public DropdownAttribute(string funcName, EUnique unique) : this(funcName, true, unique) {}
        public DropdownAttribute(bool slashAsSub, EUnique unique) : this(null, slashAsSub, unique) {}
        public DropdownAttribute(EUnique unique) : this(null, true, unique) {}
    }
}
