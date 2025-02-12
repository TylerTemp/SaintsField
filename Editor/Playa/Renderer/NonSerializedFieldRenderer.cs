using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NonSerializedFieldRenderer: AbsRenderer
    {
        private readonly bool _renderField;
        public NonSerializedFieldRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _renderField = fieldWithInfo.FieldInfo.GetCustomAttribute<ShowInInspectorAttribute>() != null;
        }

        public override void OnDestroy()
        {
        }
    }
}
