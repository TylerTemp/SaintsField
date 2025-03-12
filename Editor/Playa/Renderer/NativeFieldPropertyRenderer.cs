using SaintsField.Playa;
using UnityEditor;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativeFieldPropertyRenderer: AbsRenderer
    {
        protected bool RenderField;

        public NativeFieldPropertyRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            RenderField = fieldWithInfo.PlayaAttributes.Any(each => each is ShowInInspectorAttribute);
        }

        public override void OnDestroy()
        {
        }

        private static object GetValue(SaintsFieldWithInfo fieldWithInfo) =>
            fieldWithInfo.FieldInfo != null
                ? fieldWithInfo.FieldInfo.GetValue(fieldWithInfo.Target)
                : fieldWithInfo.PropertyInfo.GetValue(fieldWithInfo.Target);

        private static string GetName(SaintsFieldWithInfo fieldWithInfo) =>
            fieldWithInfo.PropertyInfo?.Name ?? fieldWithInfo.FieldInfo.Name;

        private static string GetNiceName(SaintsFieldWithInfo fieldWithInfo) =>
            ObjectNames.NicifyVariableName(GetName(fieldWithInfo));
    }
}
