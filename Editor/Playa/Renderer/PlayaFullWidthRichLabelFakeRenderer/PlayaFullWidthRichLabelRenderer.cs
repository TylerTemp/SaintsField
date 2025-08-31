using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.PlayaFullWidthRichLabelFakeRenderer
{
    public partial class PlayaFullWidthRichLabelRenderer: AbsRenderer
    {
        private readonly PlayaBelowRichLabelAttribute _playaBelowRichLabelAttribute;

        public PlayaFullWidthRichLabelRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, PlayaBelowRichLabelAttribute playaBelowRichLabelAttribute) : base(serializedObject, fieldWithInfo)
        {
            _playaBelowRichLabelAttribute = playaBelowRichLabelAttribute;
        }

        public override void OnDestroy()
        {
        }

        public override void OnSearchField(string searchString)
        {
        }

        public override string ToString()
        {
            return $"<AboveRichLabel {FieldWithInfo}/>";
        }

        private static (MemberInfo memberInfo, string label) GetMemberAndLabel(SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.RenderType == SaintsRenderType.ClassStruct)
            {
                return (null, fieldWithInfo.Targets[0].GetType().Name);
            }

            MemberInfo memberInfo = GetMemberInfo(fieldWithInfo);
            return (memberInfo, memberInfo.Name);
        }
    }
}
