using System.Collections.Generic;

namespace SaintsField.Interfaces
{
    public interface IPathedDropdownAttribute
    {
        public string FuncName { get; }

        public EUnique EUnique { get; }

        public PathedMode PathedMode { get; }

        public IReadOnlyList<object> Options { get; }
        public IReadOnlyList<(string path, object value)> Tuples { get; }
        public bool slashAsSub { get; }
    }
}
