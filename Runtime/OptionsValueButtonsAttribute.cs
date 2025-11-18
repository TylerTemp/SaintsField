using System.Linq;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class OptionsValueButtonsAttribute: ValueButtonsAttribute
    {
        public override Mode BehaveMode => Mode.Options;

        public OptionsValueButtonsAttribute(params object[] options)
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
