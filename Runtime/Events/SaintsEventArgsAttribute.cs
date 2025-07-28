using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField.Events
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class SaintsEventArgsAttribute: PropertyAttribute
    {
        public readonly IReadOnlyList<string> ArgNames;

        public SaintsEventArgsAttribute(params string[] argNames)
        {
            ArgNames = argNames;
        }
    }
}
