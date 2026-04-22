using SaintsField.Utils;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public static class OnLoadForRoslyn
    {
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            if (SaintsFieldConfigUtil.IsConfigLoaded)
            {
                RoslynUtil.CheckChange(SaintsFieldConfigUtil.Config);
            }
            else
            {
                SaintsFieldConfigUtil.OnConfigLoaded.AddListener(config => RoslynUtil.CheckChange(config));
            }
        }
    }
}
