using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer: SerializedFieldBaseRenderer, IMakeRenderer, IDOTweenPlayRecorder
    {
        private bool _arraySizeCondition;
        private bool _richLabelCondition;
        private bool _tableCondition;

        public ListDrawerSettingsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        public IEnumerable<AbsRenderer> MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
        }
    }
}
