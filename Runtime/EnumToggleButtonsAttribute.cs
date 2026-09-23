using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EnumToggleButtonsAttribute: PathedDropdownAttribute
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public bool NoFold;
        public EObsolete Obsolete;
        // ReSharper enable FieldCanBeMadeReadOnly.Global

        public EnumToggleButtonsAttribute(bool noFold=false, EObsolete obsolete = EObsolete.Remove)
        {
            NoFold = noFold;
            slashAsSub = false;
            Obsolete = obsolete;
        }

        public EnumToggleButtonsAttribute(EObsolete obsolete): this()
        {
            Obsolete = obsolete;
        }

        public EnumToggleButtonsAttribute(EObsolete obsolete, bool noFold): this(noFold, obsolete)
        {
        }
    }
}
