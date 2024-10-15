using System.Linq;
using System.Reflection;
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
        private readonly bool _renderField;
        public NonSerializedFieldRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(fieldWithInfo)
        {
            _renderField = fieldWithInfo.FieldInfo.GetCustomAttribute<ShowInInspectorAttribute>() != null;
        }

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        private VisualElement _fieldElement;
        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
        {
            if (!_renderField)
            {
                return (null, false);
            }

            object value = FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Target);

            VisualElement container = new VisualElement
            {
                userData = value,
                name = $"saints-field--non-serialized-field--{FieldWithInfo.FieldInfo.Name}",
            };
            VisualElement result = UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name));
            result.name = $"saints-field--non-serialized-field--value-{FieldWithInfo.FieldInfo.Name}";
            container.Add(result);

            // bool callUpdate = FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute) > 0;
            // container.RegisterCallback<AttachToPanelEvent>(_ =>
            //     container.schedule.Execute(() => WatchValueChanged(FieldWithInfo, container, callUpdate)).Every(100)
            // );

            return (_fieldElement = container, true);
        }

        protected override PreCheckResult OnUpdateUIToolKit()
        // private static void WatchValueChanged(SaintsFieldWithInfo fieldWithInfo, VisualElement container, bool callUpdate)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit();
            object userData = _fieldElement.userData;
            object value = FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Target);

            // Debug.Log($"{userData}/{value}");

            bool isEqual = Util.GetIsEqual(userData, value);

            VisualElement child = _fieldElement.Children().First();

            if (!isEqual)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NON_SERIALIZED_FIELD_RENDERER
                Debug.Log($"non serialized field update {container.name} {userData} -> {value}");
#endif
                StyleEnum<DisplayStyle> displayStyle = child.style.display;
                _fieldElement.Clear();
                _fieldElement.userData = value;
                _fieldElement.Add(child = UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name)));
                // Debug.Log($"child={child}");
                child.style.display = displayStyle;
            }

            // if(callUpdate)
            // {
            //     UpdatePreCheckUIToolkit(fieldWithInfo, child, false);
            // }
            // container.schedule.Execute(() => WatchValueChanged(fieldWithInfo, serializedObject, container, callUpdate)).Every(100);
            return preCheckResult;
        }
#endif
        public override void OnDestroy()
        {
        }

        protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
        {
            object value = FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Target);
            FieldLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name));
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return FieldHeight(FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Target), ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name));
        }

        protected override void RenderPositionTarget(Rect position, PreCheckResult preCheckResult)
        {
            object value = FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Target);
            FieldPosition(position, value, ObjectNames.NicifyVariableName(FieldWithInfo
                .FieldInfo.Name));
        }
    }
}
