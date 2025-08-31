using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class TableHeadersHideAttribute: TableHeadersAttribute
    {
        public override bool IsHide => true;

        public TableHeadersHideAttribute(params string[] headers) : base(headers)
        {
        }
    }
}
