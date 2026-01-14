using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.ShowInInspectorFieldFakeRenderer;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer
{
    public partial class ResultsRenderer: ShowInInspectorFieldRenderer
    {
        private readonly AutoRunnerWindowBase _autoRunner;

        public ResultsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            RenderField = true;
            _autoRunner = (AutoRunnerWindowBase) serializedObject.targetObject;
        }

        private struct AutoRunnerResultInfo
        {
            public AutoRunnerResult AutoRunnerResult;
            public int Index;
        }


        private static IEnumerable<(object mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResultInfo>> subGroup)> FormatResults(IReadOnlyList<AutoRunnerResult> results)
        {
            return results
                .Select((autoRunner, index) => new AutoRunnerResultInfo
                {
                    AutoRunnerResult = autoRunner,
                    Index = index,
                })
                .Where(each => each.AutoRunnerResult.FixerResult != null)
                .GroupBy(each => each.AutoRunnerResult.mainTarget)
                .Select(each => (
                    each.Key,
                    each.GroupBy(sub => sub.AutoRunnerResult.subTarget)
                ));
        }
    }
}
