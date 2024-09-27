#if UNITY_EDITOR
using System;

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

            bool quoted = (fullPart.StartsWith("\"") && fullPart.EndsWith("\"")) ||
                          (fullPart.StartsWith("'") && fullPart.EndsWith("'"));
            if (quoted)
            {
                return new FilterComparerString(FilterComparer.Equal, fullPart.Substring(1, fullPart.Length - 2));
            }


            string[] split = fullPart.Split(new[]{' '}, 2);
            string comparerPart = split[0];
            string numPart = split[1];

            if (comparerPart.StartsWith("!="))
            {
                return int.TryParse(numPart, out int num)
                    ? (FilterComparerBase)new FilterComparerInt(FilterComparer.NotEqual, num)
                    : new FilterComparerString(FilterComparer.NotEqual, numPart);
            }

            if (comparerPart.StartsWith("="))
            {
                return int.TryParse(numPart, out int num)
                    ? (FilterComparerBase)new FilterComparerInt(FilterComparer.Equal, num)
                    : new FilterComparerString(FilterComparer.Equal, numPart);
            }

            if (comparerPart.StartsWith(">="))
            {
                return new FilterComparerInt(FilterComparer.GreaterEqual, int.Parse(numPart));
            }

            if (comparerPart.StartsWith(">"))
            {
                return new FilterComparerInt(FilterComparer.Greater, int.Parse(numPart));
            }

            if (comparerPart.StartsWith("<="))
            {
                return new FilterComparerInt(FilterComparer.LessEqual, int.Parse(numPart));
            }

            // ReSharper disable once InvertIf
            if (comparerPart.StartsWith("<"))
            {
                return new FilterComparerInt(FilterComparer.Less, int.Parse(numPart));
            }

            throw new ArgumentOutOfRangeException(nameof(comparerPart), comparerPart, null);
        }
    }
}
#endif
