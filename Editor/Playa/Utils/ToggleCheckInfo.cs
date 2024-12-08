using System.Collections.Generic;
using SaintsField.Condition;

namespace SaintsField.Editor.Playa.Utils
{
    public class ToggleCheckInfo
    {
        public ToggleType Type;
        public IReadOnlyList<ConditionInfo> ConditionInfos;
        public object Target;

        public IReadOnlyList<string> Errors;
        public IReadOnlyList<bool> BoolResults;
    }
}
