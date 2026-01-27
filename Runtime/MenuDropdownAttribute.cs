using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class MenuDropdownAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string FuncName;
        public readonly bool SlashAsSub;
        public readonly EUnique EUnique;

        public MenuDropdownAttribute(string funcName=null, bool slashAsSub=true, EUnique unique=EUnique.None)
        {
            FuncName = funcName;
            SlashAsSub = slashAsSub;
            EUnique = unique;
        }

        public MenuDropdownAttribute(string funcName, EUnique unique) : this(funcName, true, unique) {}
        public MenuDropdownAttribute(bool slashAsSub, EUnique unique) : this(null, slashAsSub, unique) {}
        public MenuDropdownAttribute(EUnique unique) : this(null, true, unique) {}
    }
}
