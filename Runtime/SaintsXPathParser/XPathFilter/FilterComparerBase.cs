#if UNITY_EDITOR
using System;
using UnityEngine;

namespace SaintsField.SaintsXPathParser.XPathFilter
{
    public abstract class FilterComparerBase
    {
        public readonly FilterComparer FilterComparer;

        protected FilterComparerBase(FilterComparer filterComparer)
        {
            FilterComparer = filterComparer;
        }

        public static FilterComparerBase Parser(string fullPart)
        {
            // Debug.Log(fullPart);
            if (fullPart == "")
            {
                return new FilterComparerTruly();
            }

            if(int.TryParse(fullPart, out int fullNum))
            {
                return new FilterComparerInt(FilterComparer.Equal, fullNum);
            }

            if(fullPart == "last()")
            {
                return new FilterComparerInt(FilterComparer.Equal, -1);
            }

            string[] split = fullPart.Split(new[]{' '}, 2);
            string comparerPart = split[0];
            FilterComparer comparer = GetComPart(comparerPart);
            string numPart = split[1].Trim();

            bool quoted = (numPart.StartsWith("\"") && numPart.EndsWith("\"")) ||
                          (numPart.StartsWith("'") && numPart.EndsWith("'"));

            if (quoted)
            {
                string nonQuoted = numPart.Substring(1, numPart.Length - 2);
                // Debug.Log($"string {comparer}: {nonQuoted}");
                return new FilterComparerString(comparer, nonQuoted);
            }

            return int.TryParse(numPart, out int num)
                // ReSharper disable once RedundantCast
                ? (FilterComparerBase)new FilterComparerInt(comparer, num)
                : new FilterComparerString(comparer, numPart);
        }

        private static FilterComparer GetComPart(string comparerPart)
        {
            if (comparerPart.StartsWith("!="))
            {
                return FilterComparer.NotEqual;
            }

            if (comparerPart.StartsWith("="))
            {
                return FilterComparer.Equal;
            }

            if (comparerPart.StartsWith(">="))
            {
                return FilterComparer.GreaterEqual;
            }

            if (comparerPart.StartsWith(">"))
            {
                return FilterComparer.Greater;
            }

            if (comparerPart.StartsWith("<="))
            {
                return FilterComparer.LessEqual;
            }

            if (comparerPart.StartsWith("<"))
            {
                return FilterComparer.Less;
            }

            throw new ArgumentOutOfRangeException(nameof(comparerPart), comparerPart, null);
        }
    }
}
#endif
