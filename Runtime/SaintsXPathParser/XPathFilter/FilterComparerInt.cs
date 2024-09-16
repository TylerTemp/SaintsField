namespace SaintsField.SaintsXPathParser.XPathFilter
{
    public class FilterComparerInt: FilterComparerBase
    {
        public readonly int Value;
        public FilterComparerInt(FilterComparer filterComparer, int value) : base(filterComparer)
        {
            Value = value;
        }
    }
}
