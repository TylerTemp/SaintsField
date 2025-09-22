#if UNITY_EDITOR && SAINTSFIELD_DEBUG && SAINTSFIELD_CODE_ANALYSIS
using System.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField
{
    public class CodeAnalTest: SaintsMonoBehaviour
    {
        [Button]
        private CodeAnalysisUtils.ClassContainer[] Run()
        {
            return CodeAnalysisUtils.Parse(AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/SaintsField/Samples/Scripts/SaintsEditor/Testing/MixLayoutTest.cs")).ToArray();
        }
    }
}
#endif
