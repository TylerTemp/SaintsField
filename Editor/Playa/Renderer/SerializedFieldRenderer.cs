using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;


namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer: SerializedFieldBaseRenderer
    {
        public SerializedFieldRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        public override void OnDestroy()
        {
        }

        // private string _curXml;
        // private RichTextDrawer.RichTextChunk[] _curXmlChunks;
    }
}
