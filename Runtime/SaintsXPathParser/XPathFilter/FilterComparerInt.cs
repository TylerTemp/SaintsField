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
            switch (FilterComparer)
            {
                case FilterComparer.Equal:
                    return compareResult == 0;
                case FilterComparer.NotEqual:
                    return compareResult != 0;
                case FilterComparer.Greater:
                    return compareResult > 0;
                case FilterComparer.GreaterEqual:
                    return compareResult >= 0;
                case FilterComparer.Less:
                    return compareResult < 0;
                case FilterComparer.LessEqual:
                    return compareResult <= 0;
                case FilterComparer.None:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(FilterComparer), FilterComparer, null);
            }
        }
    }
}
#endif
