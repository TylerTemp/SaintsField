using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class AdvancedDropdownAttribute: PathedDropdownAttribute
    {
        public const float DefaultTitleHeight = 45f;

        public const float TitleHeight = DefaultTitleHeight;
        public const float MinHeight = -1f;

        public AdvancedDropdownAttribute(string funcName = null, EUnique unique = EUnique.None) : base(funcName, unique)
        {
        }

        public AdvancedDropdownAttribute(EUnique unique) : base(unique)
        {
        }
    }
}
