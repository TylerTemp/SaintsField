#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEngine;
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
            public bool IsCollection;
            public IReadOnlyList<object> OldCollection;
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
                name = NameContainer(),
            };
            // VisualElement result = UIToolkitLayout(value, GetNiceName(FieldWithInfo));
            Action<object> setter = GetSetterOrNull(FieldWithInfo);
            Type fieldType = GetFieldType(FieldWithInfo);
            VisualElement result = UIToolkitValueEdit(null, GetNiceName(FieldWithInfo), fieldType, value, setter);

            bool isCollection = fieldType.IsArray || typeof(IList).IsAssignableFrom(fieldType);
            // Debug.Log(isCollection);
            // Debug.Log(fieldType);
            // Debug.Log(typeof(IList).IsAssignableFrom(fieldType));

            if(result != null)
            {
                IReadOnlyList<object> oldCollection = isCollection && (value != null) ? ((IEnumerable)value).Cast<object>().ToArray() : null;
                result.name = NameResult();
                container.Add(result);
                container.userData = new DataPayload
                {
                    HasDrawer = true,
                    Value = value,
                    Setter = setter,
                    IsCollection = isCollection,
                    OldCollection = oldCollection,
                };
            }
            else
            {
                container.userData = new DataPayload
                {
                    HasDrawer = false,
                    Value = null,
                    Setter = setter,
                    IsCollection = isCollection,
                    OldCollection = null,
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
            if(isEqual && userData.IsCollection)
            {
                IReadOnlyList<object> oldCollection = userData.OldCollection;
                if (oldCollection == null && value == null)
                {
                }
                else if (oldCollection != null && value == null)
                {
                    isEqual = false;
                }
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                else if (oldCollection == null && value != null)
                {
                    isEqual = false;
                }
                else
                {
                    isEqual = oldCollection.SequenceEqual(((IEnumerable)value).Cast<object>());
                    // Debug.Log($"sequence equal: {isEqual}");
                }
            }
            // bool isEqual = userData.HasDrawer && (userData.Value == value || ReferenceEquals(userData.Value, value));
            VisualElement fieldElementOrNull = container.Q<VisualElement>(NameResult());

            // Debug.Log($"isEqual={isEqual}");

            if (!isEqual)
            {
                // Debug.Log($"fieldElementOrNull={fieldElementOrNull?.name}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                Debug.Log($"native property update {userData.Value} -> {value}");
#endif
                userData.Value = value;
                if (userData.IsCollection)
                {
                    userData.OldCollection = ((IEnumerable)value)?.Cast<object>().ToArray();
                }
                VisualElement result = UIToolkitValueEdit(fieldElementOrNull, GetNiceName(FieldWithInfo), GetFieldType(FieldWithInfo), value, userData.Setter);
                if(result != null)
                {
                    result.name = NameResult();
                    container.Clear();
                    container.Add(result);
                    userData.HasDrawer = true;
                }
                else if(fieldElementOrNull == null)
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
