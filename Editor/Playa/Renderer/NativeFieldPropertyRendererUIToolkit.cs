#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativeFieldPropertyRenderer
    {
        private VisualElement _fieldElement;

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
        {
            if (!RenderField)
            {
                return (null, false);
            }

            object value = GetValue(FieldWithInfo);

            VisualElement container = new VisualElement
            {
                userData = value,
                name = $"saints-field--native-property-field--{GetName(FieldWithInfo)}",
            };
            VisualElement result =
                UIToolkitLayout(value, GetNiceName(FieldWithInfo));
            if(result != null)
            {
                container.Add(result);
            }

            // _callUpdate = FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute) > 0;
            // container.RegisterCallback<AttachToPanelEvent>(_ =>
            //     container.schedule.Execute(() => WatchValueChanged(FieldWithInfo, container, callUpdate)).Every(100)
            // );

            return (_fieldElement = container, result != null);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
            // private static void WatchValueChanged(SaintsFieldWithInfo fieldWithInfo,  VisualElement container, bool callUpdate)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit(root);
            if (!RenderField)
            {
                return preCheckResult;
            }

            if (_fieldElement == null)
            {
                return preCheckResult;
            }

            object userData = _fieldElement.userData;
            object value = GetValue(FieldWithInfo);

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
                _fieldElement.Add(child = UIToolkitLayout(value, GetNiceName(FieldWithInfo)));
                child.style.display = displayStyle;
            }

            return preCheckResult;
            // container.schedule.Execute(() => WatchValueChanged(fieldWithInfo, serializedObject, container, callUpdate)).Every(100);
        }
    }
}
#endif
