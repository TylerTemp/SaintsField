using UnityEditor;

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
namespace SaintsField.Editor.Drawers.Addressable
{
    public static class AddressableUtil
    {
        public const string ErrorNoSettings = "Addressable has no settings created yet.";
        public static void OpenGroupEditor() => EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
    }
}
#endif
