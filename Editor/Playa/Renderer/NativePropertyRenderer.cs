using UnityEditor;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class NativePropertyRenderer: AbsRenderer
    {
        public NativePropertyRenderer(UnityEditor.Editor editor, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(editor, fieldWithInfo, tryFixUIToolkit)
        {
        }
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            object value = FieldWithInfo.PropertyInfo.GetValue(SerializedObject.targetObject);
            VisualElement child = UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));

            VisualElement container = new VisualElement
            {
                userData = value,
            };
            container.Add(child);

            container.RegisterCallback<AttachToPanelEvent>(_ => WatchValueChanged(container));

            return container;
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
            // NaughtyEditorGUI.NativeProperty_Layout(serializedObject.targetObject, fieldWithInfo.propertyInfo);
            object value = FieldWithInfo.PropertyInfo.GetValue(SerializedObject.targetObject);
            FieldLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
            // FieldLayout(serializedObject.targetObject, ObjectNames.NicifyVariableName(fieldWithInfo.fieldInfo.Name));
        }

    }
}
