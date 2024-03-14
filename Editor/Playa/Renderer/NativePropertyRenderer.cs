using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class NativePropertyRenderer: AbsRenderer
    {
        public NativePropertyRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(serializedObject, fieldWithInfo, tryFixUIToolkit)
        {
        }
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            object value = FieldWithInfo.PropertyInfo.GetValue(SerializedObject.targetObject);
            VisualElement child = UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));

            VisualElement result = new VisualElement
            {
                userData = value,
            };
            result.Add(child);

            result.RegisterCallback<AttachToPanelEvent>(_ => WatchValueChanged(result));
            if (FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute || each is PlayaHideIfAttribute) > 0)
            {
                result.RegisterCallback<AttachToPanelEvent>(_ => result.schedule.Execute(() => UIToolkitOnUpdate(result, false)).Every(100));
            }

            return result;
        }

        private void WatchValueChanged(VisualElement container)
        {
            object userData = container.userData;
            object value = FieldWithInfo.PropertyInfo.GetValue(SerializedObject.targetObject);
            if (userData != value)
            {
                container.Clear();
                container.userData = value;
                container.Add(UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                    .PropertyInfo.Name)));
            }
            container.schedule.Execute(() => WatchValueChanged(container)).Every(100);
        }
#endif
        public override void Render()
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            // NaughtyEditorGUI.NativeProperty_Layout(serializedObject.targetObject, fieldWithInfo.propertyInfo);
            object value = FieldWithInfo.PropertyInfo.GetValue(SerializedObject.targetObject);
            FieldLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
            // FieldLayout(serializedObject.targetObject, ObjectNames.NicifyVariableName(fieldWithInfo.fieldInfo.Name));
        }

        public override float GetHeight()
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return 0;
            }

            return SaintsPropertyDrawer.SingleLineHeight;
        }

        public override void RenderPosition(Rect position)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            object value = FieldWithInfo.PropertyInfo.GetValue(SerializedObject.targetObject);
            FieldPosition(position, value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
        }
    }
}
