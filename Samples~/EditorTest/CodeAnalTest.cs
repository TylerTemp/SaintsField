#if UNITY_EDITOR && SAINTSFIELD_DEBUG && SAINTSFIELD_CODE_ANALYSIS
using System.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;

namespace SaintsField
{
    public class CodeAnalTest: SaintsMonoBehaviour
    {
        [Button]
        private CodeAnalysisUtils.ClassContainer[] Run()
        {
            return CodeAnalysisUtils.Parse().ToArray();
        }

        [Button]
        private int Re() => 5;
    }
}
#endif
