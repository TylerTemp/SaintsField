using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer: SerializedFieldBaseRenderer
    {
        private PropertyField _result;

        private VisualElement _fieldElement;
        private bool _arraySizeCondition;
        private bool _richLabelCondition;
        private bool _tableCondition;

        public ListDrawerSettingsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

    }
}
