using System.Linq;
using SaintsField.Editor.Playa.Renderer;
using UnityEditor;

namespace SaintsField.Editor.Playa.SaintsEditorWindowUtils
{
    [CustomEditor(typeof(SaintsEditorWindow), editorForChildClasses: true)]
    public class SaintsEditorWindowSpecialEditor: SaintsEditor
    {
        public override bool RequiresConstantRepaint() =>
#if SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
            false
#else
            false
#endif
        ;

        public override void OnEnable()
        {
            EditorShowMonoScript = false;
            base.OnEnable();
        }

        public override AbsRenderer MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            if(fieldWithInfo.RenderType == SaintsRenderType.SerializedField && fieldWithInfo.FieldInfo.Name == "m_SerializedDataModeController")
            {
                return null;
            }

            if (fieldWithInfo.PlayaAttributes.Any(each => each is SaintsEditorWindow.WindowInlineEditorAttribute))
            {
                // Debug.Log(fieldWithInfo);
                return new WindowInlineEditorRenderer(fieldWithInfo);
            }

            // Debug.Log($"{fieldWithInfo.RenderType}/{fieldWithInfo.FieldInfo?.Name}/{string.Join(",", fieldWithInfo.PlayaAttributes)}");
            return base.MakeRenderer(so, fieldWithInfo);
            // return null;
        }
    }


}
