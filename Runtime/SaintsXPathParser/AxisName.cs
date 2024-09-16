using System.Collections.Generic;

namespace SaintsField.SaintsXPathParser
{
    public class AxisName
    {
        public string StartsWith;
        public string EndsWith;
        public IReadOnlyList<string> Contains;
        public bool NameAny;
        public string ExactMatch;

        public bool Descendant;  // step starts with `//`
    }
}
