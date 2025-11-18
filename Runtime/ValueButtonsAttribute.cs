using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class ValueButtonsAttribute: PathedDropdownAttribute
    {
        public ValueButtonsAttribute(string funcName = null, EUnique unique = EUnique.None): base(funcName, unique)
        {
        }

        public ValueButtonsAttribute(EUnique unique) : base(unique) {}
    }
}
