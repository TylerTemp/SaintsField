using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.RealTimeCalculatorFakeRenderer
{
    public partial class RealTimeCalculatorRenderer: AbsRenderer
    {
        private readonly SerializedObject _serializedObject;

        public RealTimeCalculatorRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _serializedObject = serializedObject;
        }

        public override void OnDestroy()
        {

        }

        public override void OnSearchField(string searchString)
        {
            throw new System.NotImplementedException();
        }
    }
}
