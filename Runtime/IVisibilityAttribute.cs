using System.Collections.Generic;
using SaintsField.Condition;

namespace SaintsField
{
    public interface IVisibilityAttribute
    {
        bool IsShow { get; }
        IReadOnlyList<ConditionInfo> ConditionInfos { get; }
        EMode EditorMode { get; }
    }
}
