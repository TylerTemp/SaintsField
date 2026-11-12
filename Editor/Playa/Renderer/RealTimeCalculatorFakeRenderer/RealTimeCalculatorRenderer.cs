using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.Events;
#endif

namespace SaintsField.Editor.Playa.Renderer.RealTimeCalculatorFakeRenderer
{
    public partial class RealTimeCalculatorRenderer: AbsRenderer
    {
        protected override bool AllowGuiColor => true;

        private readonly SerializedObject _serializedObject;

        public RealTimeCalculatorRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _serializedObject = serializedObject;
        }

#if UNITY_2021_3_OR_NEWER
        private System.Func<string, bool> _searchFunc;
#endif

        public override bool OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            return _searchFunc?.Invoke(searchString) ?? false;
#endif

#pragma warning disable CS0162 // Unreachable code detected
            // ReSharper disable once HeuristicUnreachableCode
            return false;
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
