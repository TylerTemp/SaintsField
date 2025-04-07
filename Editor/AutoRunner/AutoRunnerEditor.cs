using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Playa.SaintsEditorWindowUtils;
using UnityEditor;

namespace SaintsField.Editor.AutoRunner
{
    [CustomEditor(typeof(AutoRunnerWindowBase), true)]
    public class AutoRunnerEditor: SaintsEditorWindowSpecialEditor
    {
        public override IEnumerable<AbsRenderer> MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.PlayaAttributes.Any(each => each is AutoRunnerWindowResultsAttribute))
            {
                yield return new ResultsRenderer(so, fieldWithInfo);
                yield break;
            }

            // Debug.Log($"{fieldWithInfo.RenderType}/{fieldWithInfo.FieldInfo?.Name}/{string.Join(",", fieldWithInfo.PlayaAttributes)}");
            foreach (AbsRenderer absRenderer in base.MakeRenderer(so, fieldWithInfo))
            {
                yield return absRenderer;
            }
        }
    }
}
