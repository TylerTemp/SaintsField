namespace SaintsField.SaintsXPathParser.XPathFilter
{
    public class FilterComparerString : FilterComparerBase
    {
        public readonly string Value;

        public FilterComparerString(FilterComparer filterComparer, string value) : base(filterComparer)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"Str{{{FilterComparer} {Value}}}";
        }
    }
}
