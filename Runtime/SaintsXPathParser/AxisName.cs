using System.Collections.Generic;

namespace SaintsField.SaintsXPathParser
{
    public struct AxisName
    {
        public string StartsWith;
        public string EndsWith;
        public IReadOnlyList<string> Contains;
        public bool NameAny;
        public string ExactMatch;
    }
}
