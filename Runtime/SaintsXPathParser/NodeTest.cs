#if UNITY_EDITOR
using System.Collections.Generic;

namespace SaintsField.SaintsXPathParser
{
    public struct NodeTest
    {
        public string StartsWith;
        public string EndsWith;
        public IReadOnlyList<string> Contains;
        public bool NameEmpty;
        public bool NameAny;
        public string ExactMatch;

        public override string ToString()
        {
            return "Match{" +
                   (NameEmpty ? "NameEmpty" : "") +
                   (NameAny ? "NameAny" : "") +
                   (ExactMatch != null ? $"ExactMatch: {ExactMatch}" : "") +
                   (StartsWith != null ? $"StartsWith: {StartsWith}" : "") +
                   (EndsWith != null ? $"EndsWith: {EndsWith}" : "") +
                   (Contains != null ? $"Contains: {string.Join(", ", Contains)}" : "") +
                   "}";
        }
    }
}
#endif
