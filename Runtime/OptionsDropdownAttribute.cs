using System.Diagnostics;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.Parameter)]
    public class OptionsDropdownAttribute: DropdownAttribute
    {
        public override Mode BehaveMode => Mode.Options;

        public OptionsDropdownAttribute(params object[] options)
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
