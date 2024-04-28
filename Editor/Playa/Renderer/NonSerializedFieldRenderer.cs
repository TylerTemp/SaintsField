using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class NonSerializedFieldRenderer: AbsRenderer
    {
        public NonSerializedFieldRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(serializedObject, fieldWithInfo, tryFixUIToolkit)
        {
        }

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            // Debug.Log(FieldWithInfo.FieldInfo.Name);
            // Debug.Log($"Non Serialized Field {FieldWithInfo.FieldInfo.Name}");
            object value = FieldWithInfo.FieldInfo.GetValue(SerializedObject.targetObject);
            // need this to update when the field is disabled/hide
            VisualElement container = new VisualElement
            {
                name = $"saints-field--non-serialized-field--{FieldWithInfo.FieldInfo.Name}",
            };
            VisualElement result = UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name));
            result.name = $"saints-field--non-serialized-field--value-{FieldWithInfo.FieldInfo.Name}";
            container.Add(result);

            container.RegisterCallback<AttachToPanelEvent>(_ =>
                container.schedule.Execute(() => WatchValueChanged(FieldWithInfo, SerializedObject, container, FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute) > 0)).Every(100)
            );

            return container;
        }

        private static void WatchValueChanged(SaintsFieldWithInfo fieldWithInfo, SerializedObject serializedObject, VisualElement container, bool callUpdate)
        {
            object userData = container.userData;
            object value = fieldWithInfo.FieldInfo.GetValue(serializedObject.targetObject);

            bool isEqual = Util.GetIsEqual(userData, value);

            VisualElement child = container.Children().First();
            // Debug.Log(container.name);
            // Debug.Log(container.Children().Count());
            // if (userData != value)
            if (!isEqual)
            {
                // Debug.Log($"update {container.name} {userData} -> {value}");
                StyleEnum<DisplayStyle> displayStyle = child.style.display;
                container.Clear();
                container.userData = value;
                container.Add(child = UIToolkitLayout(value, ObjectNames.NicifyVariableName(fieldWithInfo.FieldInfo.Name)));
                // Debug.Log($"child={child}");
                child.style.display = displayStyle;
            }

            if(callUpdate)
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

            object value = FieldWithInfo.FieldInfo.GetValue(SerializedObject.targetObject);
            FieldLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .FieldInfo.Name));
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

            object value = FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Target);
            FieldPosition(position, value, ObjectNames.NicifyVariableName(FieldWithInfo
                .FieldInfo.Name));
        }
    }
}
