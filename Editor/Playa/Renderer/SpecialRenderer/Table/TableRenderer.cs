using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.Table
{
    public partial class TableRenderer: SerializedFieldBaseRenderer
    {
        public TableRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }
    }
}
