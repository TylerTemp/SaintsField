using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.Renderer.PlayaFullWidthRichLabelFakeRenderer
{
    public partial class PlayaFullWidthRichLabelRenderer: AbsRenderer
    {
        protected override bool AllowGuiColor => true;

        private readonly BelowTextAttribute _playaBelowRichLabelAttribute;

        public PlayaFullWidthRichLabelRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, BelowTextAttribute playaBelowRichLabelAttribute) : base(serializedObject, fieldWithInfo)
        {
            _playaBelowRichLabelAttribute = playaBelowRichLabelAttribute;
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
