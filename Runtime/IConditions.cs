using System.Collections.Generic;
using SaintsField.Condition;

namespace SaintsField
{
    public interface IConditions
    {
        IReadOnlyList<ConditionInfo> ConditionInfos { get; }
    }
}
