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
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
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

        public bool CompareToBool(bool source)
        {
            string sourceStr = source ? "true" : "false";
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (FilterComparer)
            {
                case FilterComparer.Equal:
                    return sourceStr == Value.ToLower().Trim();
                case FilterComparer.NotEqual:
                    return sourceStr != Value.ToLower().Trim();
                default:
                    return false;
            }
        }
    }
}
#endif
