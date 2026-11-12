using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldBareRenderer: AbsRenderer
    {

        public SerializedFieldBareRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            if (fieldWithInfo.PlayaAttributes.Any(each => each is DefaultExpandAttribute))
            {
                fieldWithInfo.SerializedProperty.isExpanded = true;
            }
        }

        protected override bool AllowGuiColor => false;

#if UNITY_2021_3_OR_NEWER
        private System.Func<string, bool> _searchFunc;
#endif
        public override bool OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            return _searchFunc?.Invoke(searchString) ?? false;
#endif

#pragma warning disable CS0162 // Unreachable code detected
            // ReSharper disable once HeuristicUnreachableCode
            return false;
#pragma warning restore CS0162 // Unreachable code detected
        }

        public override string ToString()
        {
            return $"<SerializedBare {FieldWithInfo.SerializedProperty.propertyPath}/>";
        }
    }
}
