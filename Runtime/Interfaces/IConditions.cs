using System.Collections.Generic;
using SaintsField.Condition;

namespace SaintsField.Interfaces
{
    public interface IConditions
    {
        IReadOnlyList<ConditionInfo> ConditionInfos { get; }
    }
}
