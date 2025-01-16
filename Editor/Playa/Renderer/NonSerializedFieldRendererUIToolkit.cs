#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NonSerializedFieldRenderer
    {
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

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
            // private static void WatchValueChanged(SaintsFieldWithInfo fieldWithInfo, VisualElement container, bool callUpdate)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit(root);

            if (_fieldElement is null)
            {
                return preCheckResult;
            }

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
                _fieldElement.Add(child =
                    UIToolkitLayout(value, ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name)));
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
    }
}
#endif
