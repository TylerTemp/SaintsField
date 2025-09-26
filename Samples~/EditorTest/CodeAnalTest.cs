#if UNITY_EDITOR && SAINTSFIELD_DEBUG && SAINTSFIELD_CODE_ANALYSIS
using System.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField
{
    public class CodeAnalTest: SaintsMonoBehaviour
    {
        public MonoScript ms;

        [Button]
        private CodeAnalysisUtils.ClassContainer Run()
        {
            return CodeAnalysisUtils.Parse(ms).First();
        }
    }
}
#endif
