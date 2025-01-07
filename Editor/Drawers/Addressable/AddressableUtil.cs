#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE

using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.GUI;

namespace SaintsField.Editor.Drawers.Addressable
{
    public static class AddressableUtil
    {
        public const string ErrorNoSettings = "Addressable has no settings created yet.";
        public static void OpenGroupEditor() => EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
        public static void OpenLabelEditor()
        {
            LabelWindow window = UnityEditor.EditorWindow.GetWindow<LabelWindow>();
            window.Intialize(AddressableAssetSettingsDefaultObject.GetSettings(false));
            window.Show();
        }


    }
}
#endif
