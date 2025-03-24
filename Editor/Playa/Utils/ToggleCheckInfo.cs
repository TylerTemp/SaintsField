using System;
using System.Collections.Generic;
using SaintsField.Condition;

namespace SaintsField.Editor.Playa.Utils
{
    public readonly struct ToggleCheckInfo
    {
        public readonly ToggleType Type;
        public readonly IReadOnlyList<ConditionInfo> ConditionInfos;
        public readonly object Target;

        public readonly IReadOnlyList<string> Errors;
        public readonly IReadOnlyList<bool> BoolResults;

        public ToggleCheckInfo(ToggleType type, IReadOnlyList<ConditionInfo> conditionInfos, object target)
        {
            Type = type;
            ConditionInfos = conditionInfos;
            Target = target;

            Errors = Array.Empty<string>();
            BoolResults = Array.Empty<bool>();
        }

        public ToggleCheckInfo(ToggleCheckInfo otherInfo, IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults)
        {
            Type = otherInfo.Type;
            ConditionInfos = otherInfo.ConditionInfos;
            Target = otherInfo.Target;
            Errors = errors;
            BoolResults = boolResults;
        }
    }
}
