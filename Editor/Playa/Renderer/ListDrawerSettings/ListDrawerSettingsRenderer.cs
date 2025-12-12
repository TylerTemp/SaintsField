using System.Collections.Generic;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer: SerializedFieldBaseRenderer, IMakeRenderer, IDOTweenPlayRecorder
    {
        private bool _arraySizeCondition;
        private bool _richLabelCondition;
        private bool _tableCondition;

        public ListDrawerSettingsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        public IEnumerable<IReadOnlyList<AbsRenderer>> MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
        }
    }
}
