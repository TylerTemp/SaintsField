using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer;
using UnityEditor;
using System;
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

        private static (AutoRunnerResult value, int index)[] GetFixableResultsWithIndex(
            IReadOnlyList<AutoRunnerResult> results)
        {
            return results
                .Select((value, index) => (value, index))
                .Where(each => each.value.FixerResult?.CanFix ?? false)
                .Reverse()
                .ToArray();
        }

        private static bool TryRunFix(AutoRunnerResult autoRunnerResult)
        {
            AutoRunnerFixerResult fixerResult = autoRunnerResult.FixerResult;
            if (fixerResult == null)
            {
                return false;
            }

            try
            {
                fixerResult.Callback();
                return true;
            }
            catch (Exception e)
            {
                fixerResult.ExecError = e.Message;
                return false;
            }
        }

        private void RunFixAndRemove(AutoRunnerResultInfo autoRunnerResultInfo)
        {
            if (TryRunFix(autoRunnerResultInfo.AutoRunnerResult))
            {
                _autoRunner.Results.RemoveAt(autoRunnerResultInfo.Index);
            }
        }

        private void RunFixAllAndRemove((AutoRunnerResult value, int index)[] canFixWithIndex)
        {
            List<int> toRemoveIndex = new List<int>();
            foreach ((AutoRunnerResult autoRunnerResult, int index) in canFixWithIndex)
            {
                if (TryRunFix(autoRunnerResult))
                {
                    toRemoveIndex.Add(index);
                }
            }

            foreach (int index in toRemoveIndex.OrderByDescending(each => each))
            {
                _autoRunner.Results.RemoveAt(index);
            }
        }
    }
}
