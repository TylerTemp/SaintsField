﻿using UnityEditor;

namespace SaintsField.Editor.Playa
{
#if SAINTSFIELD_NAUGHYTATTRIBUTES && SAINTSFIELD_SAINTS_EDITOR_APPLY
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.MonoBehaviour), true)]
    public class ApplySaintsMonoBehaviorEditor : SaintsEditor
    {
        // should we try to fix the UI Toolkit PropertyField label unmatched width issue?
        protected override bool TryFixUIToolkit =>
#if SAINTSFIELD_SAINTS_EDITOR_UI_TOOLKIT_LABEL_FIX_DISABLE
            false
#else
            true
#endif
        ;

        // should IMGUI constant repaint? The `ProgressBar` and `Rate` will look much better
        public override bool RequiresConstantRepaint() =>
#if SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
            false
#else
            true
#endif
        ;
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.ScriptableObject), true)]
    public class ApplySaintsScriptableObjectEditor : SaintsEditor
    {
        // should we try to fix the UI Toolkit PropertyField label unmatched width issue?
        protected override bool TryFixUIToolkit =>
#if SAINTSFIELD_SAINTS_EDITOR_UI_TOOLKIT_LABEL_FIX_DISABLE
            false
#else
            true
#endif
        ;

        // should IMGUI constant repaint? The `ProgressBar` and `Rate` will look much better
        public override bool RequiresConstantRepaint() =>
#if SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
            false
#else
            true
#endif
        ;
        ;
    }
#elif SAINTSFIELD_SAINTS_EDITOR_APPLY
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class ApplySaintsEditor : SaintsEditor
    {
        // should we try to fix the UI Toolkit PropertyField label unmatched width issue?
        protected override bool TryFixUIToolkit =>
#if SAINTSFIELD_SAINTS_EDITOR_UI_TOOLKIT_LABEL_FIX_DISABLE
            false
#else
            true
#endif
        ;

        // should IMGUI constant repaint? The `ProgressBar` and `Rate` will look much better
        public override bool RequiresConstantRepaint() =>
#if SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
            false
#else
            true
#endif
        ;
    }
#endif
}