using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer: SerializedFieldBaseRenderer
    {
        private bool _arraySizeCondition;
        private bool _richLabelCondition;
        private bool _tableCondition;

        public ListDrawerSettingsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        private static IEnumerable<Object> CanDrop(IEnumerable<Object> targets, Type elementType)
        {
            return targets.Where(each => Util.GetTypeFromObj(each, elementType));
        }
    }
}
