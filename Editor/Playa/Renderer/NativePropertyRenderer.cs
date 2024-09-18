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
    public class NativePropertyRenderer: AbsRenderer
    {
        private readonly bool _renderField;

        public NativePropertyRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(fieldWithInfo)
        {
            _renderField = fieldWithInfo.PropertyInfo.GetCustomAttribute<ShowInInspectorAttribute>() != null;
        }
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        private VisualElement _fieldElement;
        // private bool _callUpdate;
        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
        {
            if (!_renderField)
            {
                return (null, false);
            }

            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);

            VisualElement container = new VisualElement
            {
                userData = value,
                name = $"saints-field--native-property--{FieldWithInfo.PropertyInfo.Name}",
            };
            VisualElement result = UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name));
            container.Add(result);

            // _callUpdate = FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute) > 0;
            // container.RegisterCallback<AttachToPanelEvent>(_ =>
            //     container.schedule.Execute(() => WatchValueChanged(FieldWithInfo, container, callUpdate)).Every(100)
            // );

            return (_fieldElement = container, true);
        }

        protected override PreCheckResult OnUpdateUIToolKit()
        // private static void WatchValueChanged(SaintsFieldWithInfo fieldWithInfo,  VisualElement container, bool callUpdate)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit();
            if (!_renderField)
            {
                return preCheckResult;
            }

            object userData = _fieldElement.userData;
            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);

            bool isEqual = Util.GetIsEqual(userData, value);

            VisualElement child = _fieldElement.Children().First();

            if (!isEqual)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                Debug.Log($"native property update {container.name} {userData} -> {value}");
#endif
                StyleEnum<DisplayStyle> displayStyle = child.style.display;
                _fieldElement.Clear();
                _fieldElement.userData = value;
                _fieldElement.Add(child = UIToolkitLayout(value,
                    ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name)));
                child.style.display = displayStyle;
            }

            return preCheckResult;
            // container.schedule.Execute(() => WatchValueChanged(fieldWithInfo, serializedObject, container, callUpdate)).Every(100);
        }
#endif
        public override void OnDestroy()
        {
        }

        protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
        {
            if (!_renderField)
            {
                return;
            }

            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);
            FieldLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!_renderField)
            {
                return 0f;
            }
            return FieldHeight(FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target), ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name));
        }

        protected override void RenderPositionTarget(Rect position, PreCheckResult preCheckResult)
        {
            if (!_renderField)
            {
                return;
            }

            object value = FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Target);
            FieldPosition(position, value, ObjectNames.NicifyVariableName(FieldWithInfo
                .PropertyInfo.Name));
        }
    }
}
