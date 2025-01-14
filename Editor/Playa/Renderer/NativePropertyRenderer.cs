using System.Reflection;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Linq;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativePropertyRenderer: AbsRenderer
    {
        protected bool RenderField;

        public NativePropertyRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            RenderField = fieldWithInfo.PlayaAttributes.Any(each => each is ShowInInspectorAttribute);
        }
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        // private bool _callUpdate;

#endif
        public override void OnDestroy()
        {
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!RenderField)
            {
                return 0f;
            }
            return FieldHeight(FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target), ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name));
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!RenderField)
            {
                return;
            }

            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);
            FieldPosition(position, value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
        }
    }
}
