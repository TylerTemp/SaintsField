using SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.SaintsEditorWindowUtils;
using UnityEditor;

namespace SaintsField.Editor.AutoRunner
{
    [CustomEditor(typeof(AutoRunnerWindow))]
    public class AutoRunnerEditor: SaintsEditorWindowSpecialEditor
    {
        public override AbsRenderer MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.FieldInfo?.Name == "results")
            {
                return new ResultsRenderer(so, fieldWithInfo);
            }

            // Debug.Log($"{fieldWithInfo.RenderType}/{fieldWithInfo.FieldInfo?.Name}/{string.Join(",", fieldWithInfo.PlayaAttributes)}");
            return base.MakeRenderer(so, fieldWithInfo);
        }
    }
}
