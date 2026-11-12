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
