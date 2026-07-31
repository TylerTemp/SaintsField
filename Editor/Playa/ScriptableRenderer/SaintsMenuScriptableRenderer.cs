using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;

namespace SaintsField.Editor.Playa.ScriptableRenderer
{
    public static class SaintsMenuScriptableRenderer
    {
        // ReSharper disable once InconsistentNaming
        private const string SAINTSFIELD_SAINTS_EDITOR_SCRIPTABLE_RENDERER_APPLY = "SAINTSFIELD_SAINTS_EDITOR_SCRIPTABLE_RENDERER_APPLY";
        private const string EnableSaintsEditorToScriptableRendererPath = RuntimeUtil.MenuRoot + "Enable SaintsEditor To Scriptable Rendererer";
        [MenuItem(EnableSaintsEditorToScriptableRendererPath, priority = SaintsMenu.EnableSaintsEditorToTargetPriority)]
        public static void EnableSaintsEditorToScriptableRenderer()
        {
            SaintsMenu.
#if SAINTSFIELD_NETCODE_GAMEOBJECTS_DISABLED
                RemoveCompileDefine
#else
                AddCompileDefine
#endif
                    (SAINTSFIELD_SAINTS_EDITOR_SCRIPTABLE_RENDERER_APPLY);
        }

        [InitializeOnLoadMethod]
        private static void Checkmark()
        {
            Menu.SetChecked(EnableSaintsEditorToScriptableRendererPath,
#if SAINTSFIELD_SAINTS_EDITOR_SCRIPTABLE_RENDERER_APPLY
                true
#else
                false
#endif
            );
        }
    }
}
