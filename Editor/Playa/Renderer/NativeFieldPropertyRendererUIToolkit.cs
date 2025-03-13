#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Linq;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class NativeFieldPropertyRenderer
    {
        // private VisualElement _fieldElement;

        private string NameContainer() => $"saints-field--native-property-field--{GetName(FieldWithInfo)}";
        private string NameResult() => $"saints-field--native-property-field--{GetName(FieldWithInfo)}-result";

        private class DataPayload
        {
            public bool HasDrawer;
            public Action<object> Setter;
            public object Value;
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
        {
            if (!RenderField)
            {
                return (null, false);
            }

            object value = GetValue(FieldWithInfo);

            VisualElement container = new VisualElement
            {
                style =
                {
                    borderLeftWidth = 2,
                    borderRightWidth = 2,
                    borderLeftColor = EColor.EditorEmphasized.GetColor(),
                    borderRightColor = EColor.EditorEmphasized.GetColor(),
                    borderTopLeftRadius = 3,
                    borderBottomLeftRadius = 3,
                    marginLeft = 1,
                    marginRight = 1,
                },
                name = $"saints-field--native-property-field--{GetName(FieldWithInfo)}",
            };
            // VisualElement result = UIToolkitLayout(value, GetNiceName(FieldWithInfo));
            Action<object> setter = GetSetterOrNull(FieldWithInfo);
            VisualElement result = UIToolkitValueEdit(null, GetNiceName(FieldWithInfo), GetFieldType(FieldWithInfo), value, setter);
            //
            if(result != null)
            {
                result.name = NameResult();
                container.Add(result);
                container.userData = new DataPayload
                {
                    HasDrawer = true,
                    Value = value,
                    Setter = setter,
                };
            }
            else
            {
                container.userData = new DataPayload
                {
                    HasDrawer = false,
                    Value = null,
                    Setter = setter,
                };
            }

            return (container, true);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit(root);
            if (!RenderField)
            {
                return preCheckResult;
            }

            VisualElement container= root.Q<VisualElement>(NameContainer());

            DataPayload userData = (DataPayload)container.userData;

            object value = GetValue(FieldWithInfo);
            bool isEqual = userData.HasDrawer && Util.GetIsEqual(userData.Value, value);
            VisualElement fieldElementOrNull = container.Q<VisualElement>(NameResult());

            if (!isEqual)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                Debug.Log($"native property update {container.name} {userData} -> {value}");
#endif
                userData.Value = value;
                VisualElement result = UIToolkitValueEdit(fieldElementOrNull, GetNiceName(FieldWithInfo), GetFieldType(FieldWithInfo), value, userData.Setter);
                if(result != null)
                {
                    result.name = NameResult();
                    container.Clear();
                    container.Add(result);
                    userData.HasDrawer = true;
                }
                else
                {
                    userData.HasDrawer = false;
                }

                // StyleEnum<DisplayStyle> displayStyle = child.style.display;
                // fieldElement.Clear();
                // fieldElement.userData = value;
                // fieldElement.Add(child = UIToolkitValueEdit(GetNiceName(FieldWithInfo), GetFieldType(FieldWithInfo), value, GetSetterOrNull(FieldWithInfo)));
                // child.style.display = displayStyle;
            }

            return preCheckResult;
            // container.schedule.Execute(() => WatchValueChanged(fieldWithInfo, serializedObject, container, callUpdate)).Every(100);
        }
    }
}
#endif
