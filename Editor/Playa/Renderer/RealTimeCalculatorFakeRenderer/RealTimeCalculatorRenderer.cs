using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.Events;
#endif

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

#if UNITY_2021_3_OR_NEWER
        private readonly UnityEvent<string> _onSearchFieldUIToolkit = new UnityEvent<string>();
#endif

        public override void OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            _onSearchFieldUIToolkit.Invoke(searchString);
#endif
        }
    }
}
