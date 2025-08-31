using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.TableExampleS
{
    public class TableHeadersExampleInh : TableHeadersExample
    {
        protected override IEnumerable<string> ShowTableHeaders() => new[]
        {
            "String",
        };
    }
}
