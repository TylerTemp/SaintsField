using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class NativePropertyRenderer: AbsRenderer
    {
        public NativePropertyRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(serializedObject, fieldWithInfo, tryFixUIToolkit)
        {
        }
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            // Debug.Log($"Native Prop {FieldWithInfo.PropertyInfo.Name}");
            object value = FieldWithInfo.PropertyInfo.GetValue(SerializedObject.targetObject);
            VisualElement child = UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));

            VisualElement result = new VisualElement
            {
                userData = value,
                name = $"saints-field--native-property--{FieldWithInfo.PropertyInfo.Name}",
            };
            result.Add(child);

            bool callUpdate = FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute) > 0;
            // call Every in function is broken I dont know why...
            // result.RegisterCallback<AttachToPanelEvent>(_ => WatchValueChanged(FieldWithInfo, SerializedObject, result, callUpdate));
            result.RegisterCallback<AttachToPanelEvent>(_ =>
                result.schedule.Execute(() => WatchValueChanged(FieldWithInfo, SerializedObject, result, callUpdate)).Every(100)
            );

            return result;
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

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        private static void WatchValueChanged(SaintsFieldWithInfo fieldWithInfo, SerializedObject serializedObject,  VisualElement container, bool callUpdate)
        {
            object userData = container.userData;
            object value = fieldWithInfo.PropertyInfo.GetValue(serializedObject.targetObject);

            bool isEqual = Util.GetIsEqual(userData, value);

            VisualElement child = container.Children().First();
            // if (userData != value)
            if (!isEqual)
            {
                Debug.Log($"update {container.name} {userData} -> {value}");
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
