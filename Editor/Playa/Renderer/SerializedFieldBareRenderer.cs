using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldBareRenderer: AbsRenderer
    {
        public SerializedFieldBareRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            if (fieldWithInfo.PlayaAttributes.Any(each => each is ArrayDefaultExpandAttribute))
            {
                fieldWithInfo.SerializedProperty.isExpanded = true;
            }
        }

        public override void OnDestroy()
        {

        }

        public override string ToString()
        {
            return $"<SerializedBare {FieldWithInfo.SerializedProperty.propertyPath}/>";
        }
    }
}
