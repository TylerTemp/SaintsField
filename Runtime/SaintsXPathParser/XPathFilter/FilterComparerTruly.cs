#if UNITY_EDITOR
namespace SaintsField.SaintsXPathParser.XPathFilter
{
    public class FilterComparerTruly: FilterComparerBase
    {
        public FilterComparerTruly() : base(FilterComparer.Equal)
        {
        }

        public override string ToString()
        {
            return "Truly";
        }
    }
}
#endif
