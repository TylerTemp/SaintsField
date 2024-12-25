using SaintsField.Editor.Playa.Renderer;
using UnityEditor;

namespace SaintsField.Editor.Playa
{
    public class SaintsEditorWindowSpecialEditor: SaintsEditor
    {
        public override bool RequiresConstantRepaint() =>
#if SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
            false
#else
            true
#endif
        ;

        public override AbsRenderer MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            if(fieldWithInfo.RenderType == SaintsRenderType.SerializedField && fieldWithInfo.FieldInfo.Name == "m_SerializedDataModeController")
            {
                return null;
            }
            // Debug.Log($"{fieldWithInfo.RenderType}/${fieldWithInfo.FieldInfo?.Name}");
            return base.MakeRenderer(so, fieldWithInfo);
            // return null;
        }
    }
}
