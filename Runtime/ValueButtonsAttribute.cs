using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class ValueButtonsAttribute: PathedDropdownAttribute
    {
        public bool NoFold;
        public EObsolete Obsolete;

        public ValueButtonsAttribute(string funcName = null, EUnique unique = EUnique.None, bool noFold=false,
            EObsolete obsolete = EObsolete.Remove): base(funcName, unique)
        {
            NoFold = noFold;
            Obsolete = obsolete;
            slashAsSub = false;
        }

        public ValueButtonsAttribute(EUnique unique) : this(null, unique) {}
        public ValueButtonsAttribute(string funcName) : this(funcName, EUnique.None) {}
        public ValueButtonsAttribute(bool noFold) : this(null, EUnique.None, noFold) {}
        public ValueButtonsAttribute(EObsolete obsolete) : this(null, EUnique.None, false, obsolete) {}
        public ValueButtonsAttribute(EObsolete obsolete, bool noFold) : this(null, EUnique.None, noFold, obsolete) {}
    }
}
