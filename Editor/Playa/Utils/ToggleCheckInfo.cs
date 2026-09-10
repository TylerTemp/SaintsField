using System;
using System.Collections.Generic;
using SaintsField.Condition;

namespace SaintsField.Editor.Playa.Utils
{
    public readonly struct ToggleCheckInfo
    {
        public readonly ToggleType Type;
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;
        public readonly IReadOnlyList<object> Targets;

        public readonly IReadOnlyList<string> Errors;
        public readonly IReadOnlyList<bool> BoolResults;

        public ToggleCheckInfo(ToggleType type, IReadOnlyList<ConditionInfo> conditionInfos, IReadOnlyList<object> targets)
        {
            Type = type;
            ConditionInfos = conditionInfos;
            Targets = targets;

            Errors = Array.Empty<string>();
            BoolResults = Array.Empty<bool>();
        }

        public ToggleCheckInfo(ToggleCheckInfo otherInfo, IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults)
        {
            Type = otherInfo.Type;
            ConditionInfos = otherInfo.ConditionInfos;
            Targets = otherInfo.Targets;
            Errors = errors;
            BoolResults = boolResults;
        }
    }
}
