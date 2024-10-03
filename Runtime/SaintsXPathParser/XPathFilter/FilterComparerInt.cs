using System;

#if UNITY_EDITOR
namespace SaintsField.SaintsXPathParser.XPathFilter
{
    public class FilterComparerInt: FilterComparerBase
    {
        public readonly int Value;
        public FilterComparerInt(FilterComparer filterComparer, int value) : base(filterComparer)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"Int{{{FilterComparer} {Value}}}";
        }

        public bool CompareToComparable(IComparable source)
        {
            int compareResult = source.CompareTo(Value);
            return FilterComparer switch
            {
                FilterComparer.Equal => compareResult == 0,
                FilterComparer.NotEqual => compareResult != 0,
                FilterComparer.Greater => compareResult > 0,
                FilterComparer.GreaterEqual => compareResult >= 0,
                FilterComparer.Less => compareResult < 0,
                FilterComparer.LessEqual => compareResult <= 0,
                FilterComparer.None => true,
                _ => throw new ArgumentOutOfRangeException(nameof(FilterComparer), FilterComparer, null),
            };
        }
    }
}
#endif
