using UnityEditor;

namespace SaintsField.Editor.Playa
{
    public class ApplySaintsEditorBase : SaintsEditor
    {
        // should we try to fix the UI Toolkit PropertyField label unmatched width issue?
//         protected override bool TryFixUIToolkit =>
// #if SAINTSFIELD_SAINTS_EDITOR_UI_TOOLKIT_LABEL_FIX_DISABLE
//             false
// #else
//             true
// #endif
//         ;

        // should IMGUI constant repaint? The `ProgressBar` and `Rate` will look much better
        public override bool RequiresConstantRepaint() =>
#if SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
            false
#else
            true
#endif
        ;
    }

#if SAINTSFIELD_NAUGHYTATTRIBUTES && SAINTSFIELD_SAINTS_EDITOR_APPLY
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.MonoBehaviour), true)]
    public class ApplySaintsMonoBehaviorEditor : ApplySaintsEditorBase
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.ScriptableObject), true)]
    public class ApplySaintsScriptableObjectEditor : ApplySaintsEditorBase
    {
    }
#elif SAINTSFIELD_SAINTS_EDITOR_APPLY
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class ApplySaintsEditor : ApplySaintsEditorBase
    {
    }
#endif
}
