using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.Parameter)]
    public class PairsDropdown: DropdownAttribute
    {
        public override Mode BehaveMode => Mode.Tuples;

        public PairsDropdown(params object[] tuples)
        {
            // Tuples = tuples;
            List<(string, object)> pairs = new List<(string, object)>();

            int startIndex = 0;
            if (tuples[0].GetType() == typeof(EUnique))
            {
                EUnique = (EUnique)tuples[0];
                startIndex = 1;
            }

            for (int index = startIndex; index < tuples.Length; index+=2)
            {
                string path = (string)tuples[index];
                object value = tuples[index + 1];
                pairs.Add((path, value));
            }

            Tuples = pairs;
        }
    }
}
