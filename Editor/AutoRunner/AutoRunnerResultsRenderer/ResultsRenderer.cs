using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using SaintsField.Editor.Utils;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer
{
    public partial class ResultsRenderer: NonSerializedFieldRenderer
    {
        private readonly AutoRunnerWindow _autoRunner;

        public ResultsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _autoRunner = (AutoRunnerWindow) serializedObject.targetObject;
        }

        private struct MainTarget : IEquatable<MainTarget>
        {
            public string MainTargetString;
            public bool MainTargetIsAssetPath;

            public bool Equals(MainTarget other)
            {
                return MainTargetString == other.MainTargetString && MainTargetIsAssetPath == other.MainTargetIsAssetPath;
            }

            public override bool Equals(object obj)
            {
                return obj is MainTarget other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(MainTargetString, MainTargetIsAssetPath);
            }
        }


        private static IEnumerable<(MainTarget mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResult>> subGroup)> FormatResults(IReadOnlyList<AutoRunnerResult> results)
        {
            return results
                .Where(each => each.FixerResult != null)
                .GroupBy(each => new MainTarget
                {
                    MainTargetString = each.mainTargetString,
                    MainTargetIsAssetPath = each.mainTargetIsAssetPath,
                })
                .Select(each => (
                    each.Key,
                    each.GroupBy(sub => sub.subTarget)
                ));
        }
    }
}
