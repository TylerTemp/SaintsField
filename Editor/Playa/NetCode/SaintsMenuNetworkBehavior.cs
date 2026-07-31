using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;

namespace SaintsField.Editor.Playa.NetCode
{
    public static class SaintsMenuNetworkBehavior
    {
        // ReSharper disable once InconsistentNaming
        private const string SAINTSFIELD_SAINTS_EDITOR_NETWORK_BEHAVIOR_APPLY = "SAINTSFIELD_SAINTS_EDITOR_NETWORK_BEHAVIOR_APPLY";
        private const string EnableSaintsEditorToNetworkBehaviorPath = RuntimeUtil.MenuRoot + "Enable SaintsEditor To NetworkBehavior";
        [MenuItem(EnableSaintsEditorToNetworkBehaviorPath, priority = SaintsMenu.EnableSaintsEditorToTargetPriority + 1)]
        public static void EnableSaintsEditorToNetworkBehavior()
        {
            SaintsMenu.
#if SAINTSFIELD_NETCODE_GAMEOBJECTS_DISABLED
                RemoveCompileDefine
#else
                AddCompileDefine
#endif
                    (SAINTSFIELD_SAINTS_EDITOR_NETWORK_BEHAVIOR_APPLY);
        }

        [InitializeOnLoadMethod]
        private static void Checkmark()
        {
            Menu.SetChecked(EnableSaintsEditorToNetworkBehaviorPath,
#if SAINTSFIELD_SAINTS_EDITOR_NETWORK_BEHAVIOR_APPLY
                true
#else
                false
#endif
            );
        }
    }
}
