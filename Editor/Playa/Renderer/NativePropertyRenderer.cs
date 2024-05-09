using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Playa;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class NativePropertyRenderer: AbsRenderer
    {
        public NativePropertyRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(serializedObject, fieldWithInfo)
        {
        }
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);

            VisualElement container = new VisualElement
            {
                userData = value,
                name = $"saints-field--native-property--{FieldWithInfo.PropertyInfo.Name}",
            };
            VisualElement result = UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name));
            container.Add(result);

            bool callUpdate = FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute) > 0;
            container.RegisterCallback<AttachToPanelEvent>(_ =>
                container.schedule.Execute(() => WatchValueChanged(FieldWithInfo, SerializedObject, container, callUpdate)).Every(100)
            );

            return container;
        }

        private static void WatchValueChanged(SaintsFieldWithInfo fieldWithInfo, SerializedObject serializedObject,  VisualElement container, bool callUpdate)
        {
            object userData = container.userData;
            object value = fieldWithInfo.PropertyInfo.GetValue(fieldWithInfo.Target);

            bool isEqual = Util.GetIsEqual(userData, value);

            VisualElement child = container.Children().First();

            if (!isEqual)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                Debug.Log($"native property update {container.name} {userData} -> {value}");
#endif
                StyleEnum<DisplayStyle> displayStyle = child.style.display;
                container.Clear();
                container.userData = value;
                container.Add(child = UIToolkitLayout(value, ObjectNames.NicifyVariableName(fieldWithInfo.PropertyInfo.Name)));
                child.style.display = displayStyle;
            }

            if (callUpdate)
            {
                UIToolkitOnUpdate(fieldWithInfo, child, false);
            }
            // container.schedule.Execute(() => WatchValueChanged(fieldWithInfo, serializedObject, container, callUpdate)).Every(100);
        }
#endif
        public override void OnDestroy()
        {
        }

        public override void Render()
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            // NaughtyEditorGUI.NativeProperty_Layout(serializedObject.targetObject, fieldWithInfo.propertyInfo);
            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);
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

            return FieldHeight(FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target), ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name));
        }

        public override void RenderPosition(Rect position)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);
            FieldPosition(position, value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
        }
    }
}
