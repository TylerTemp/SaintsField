using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
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
            true
#endif
        ;

        public override void OnEnable()
        {
            EditorShowMonoScript = false;
            base.OnEnable();
        }

        public override IEnumerable<AbsRenderer> MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            if(fieldWithInfo.RenderType == SaintsRenderType.SerializedField && fieldWithInfo.FieldInfo.Name == "m_SerializedDataModeController")
            {
                yield break;
            }

            SaintsEditorWindow.WindowInlineEditorAttribute windowInlineEditorAttribute = fieldWithInfo.PlayaAttributes.OfType<SaintsEditorWindow.WindowInlineEditorAttribute>().FirstOrDefault();
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (windowInlineEditorAttribute != null)
            {
                // Debug.Log(fieldWithInfo);
                yield return new WindowInlineEditorRenderer(so, fieldWithInfo, windowInlineEditorAttribute.EditorType);
                yield break;
            }

            // Debug.Log($"{fieldWithInfo.RenderType}/{fieldWithInfo.FieldInfo?.Name}/{string.Join(",", fieldWithInfo.PlayaAttributes)}");

            foreach (AbsRenderer absRenderer in base.MakeRenderer(so, fieldWithInfo))
            {
                yield return absRenderer;
            }
            // return null;
        }
    }


}
