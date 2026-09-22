using System;
using System.Collections.Generic;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ChipsAttribute: Attribute, IPlayaAttribute, IPathedDropdownAttribute
    {
        public string FuncName { get; }
        public EUnique EUnique { get; }
        public PathedMode PathedMode => PathedMode.Default;
        public IReadOnlyList<object> Options => null;
        public IReadOnlyList<(string path, object value)> Tuples => null;
        public bool slashAsSub { get; set; } = true;

        public ChipsAttribute(string callback = null)
        {
            FuncName = callback;
        }

        public ChipsAttribute(EUnique eUnique)
        {
            FuncName = null;
            EUnique = eUnique;
        }


    }
}
