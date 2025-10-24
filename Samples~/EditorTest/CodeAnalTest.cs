#if UNITY_EDITOR && SAINTSFIELD_CODE_ANALYSIS
using System.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.Compilation;

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

        // [Button]
        // private void Reload()
        // {
        //     EditorUtility.RequestScriptReload();
        // }

        // [Button]
        // private string ReloadScript()
        // {
        //     string s = AssetDatabase.GetAssetPath(ms);
        //     AssetDatabase.ImportAsset(s, ImportAssetOptions.ForceUpdate);
        //     return s;
        // }

        // [Button]
        // private void Recompile()
        // {
        //     CompilationPipeline.RequestScriptCompilation();
        // }
    }
}
#endif
