using SaintsField.Utils;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public static class OnLoadForRoslyn
    {
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            if(EditorUtility.IsPersistent(SaintsFieldConfig.instance))
            {
                RoslynUtil.CheckChange(SaintsFieldConfig.instance);
            }
        }
    }
}
