using System.Diagnostics;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class AdvancedOptionsDropdownAttribute: AdvancedDropdownAttribute
    {
        public override Mode BehaveMode => Mode.Options;

        public AdvancedOptionsDropdownAttribute(params object[] options)
        {
            if (options[0].GetType() == typeof(EUnique))
            {
                EUnique = (EUnique)options[0];
                Options = options.Skip(1).ToArray();
            }
            else
            {
                Options = options;
            }
        }
    }
}
