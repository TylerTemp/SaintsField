#if UNITY_EDITOR
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

        public bool CompareToString(string source)
        {
            switch (FilterComparer)
            {
                case FilterComparer.Equal:
                    return source == Value;
                case FilterComparer.NotEqual:
                    return source != Value;
                default:
                    return false;
            }
        }
    }
}
#endif
