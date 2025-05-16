using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa.Renderer.PlayaAboveRichLabelFakeRenderer
{
    public partial class PlayaAboveRichLabelRenderer: AbsRenderer
    {
        private readonly PlayaAboveRichLabelAttribute _playaAboveRichLabelAttribute;

        public PlayaAboveRichLabelRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, PlayaAboveRichLabelAttribute playaAboveRichLabelAttribute) : base(serializedObject, fieldWithInfo)
        {
            _playaAboveRichLabelAttribute = playaAboveRichLabelAttribute;
        }

        public override void OnDestroy()
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
                return (null, fieldWithInfo.Target.GetType().Name);
            }

            MemberInfo memberInfo = GetMemberInfo(fieldWithInfo);
            return (memberInfo, memberInfo.Name);
        }
    }
}
