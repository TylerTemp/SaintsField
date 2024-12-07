using System.Collections.Generic;
using SaintsField.Condition;

namespace SaintsField.Playa
{
    public interface ISaintsLayoutToggle: ISaintsLayoutBase
    {
        IReadOnlyList<ConditionInfo> ConditionInfos { get; }
        EMode EditorMode { get; }
    }
}
