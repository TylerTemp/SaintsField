using System;
using System.Diagnostics;
using SaintsField.Playa;

namespace SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class AutoRunnerWindowResultsAttribute: ShowInInspectorAttribute
    {

    }
}
