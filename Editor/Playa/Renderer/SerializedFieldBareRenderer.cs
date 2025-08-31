using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine.Events;

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

#if UNITY_2021_3_OR_NEWER
        private readonly UnityEvent<string> _onSearchFieldUIToolkit = new UnityEvent<string>();
#endif
        public override void OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            _onSearchFieldUIToolkit.Invoke(searchString);
#endif
        }

        public override string ToString()
        {
            return $"<SerializedBare {FieldWithInfo.SerializedProperty.propertyPath}/>";
        }
    }
}
