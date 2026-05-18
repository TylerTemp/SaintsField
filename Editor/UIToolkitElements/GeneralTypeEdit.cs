using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ReferencePicker;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Playa.Renderer.ShowInInspectorFieldFakeRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class GeneralTypeEdit: Foldout
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<GeneralTypeEdit, UxmlTraits> { }
#endif

        private enum UIToolkitValueEditPayloadState
        {
            None,
            FieldObject,
            GenericType,
        }

        private Type UnityObjectOverrideType;
        private UIToolkitValueEditPayloadState State;
        private bool IsFullFilled;

        private readonly RichTextDrawer _richTextDrawer;
        private readonly UIToolkitUtils.DropdownButtonField _dropdownBtn;
        private readonly VisualElement _checkMark;
        private readonly bool _labelGrayColor;
        private readonly bool _inHorizontalLayout;
        private readonly IReadOnlyList<Attribute> _allAttributes;
        private readonly IRichTextTagProvider _richTextTagProvider;
        private readonly string _foldoutViewKey;

        public GeneralTypeEdit()
        {
        }

        public GeneralTypeEdit(string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            this.value = false;

            _richTextDrawer = new RichTextDrawer();
            _labelGrayColor = labelGrayColor;
            _inHorizontalLayout = inHorizontalLayout;
            _allAttributes = allAttributes;
            _richTextTagProvider = richTextTagProvider;
            _foldoutViewKey = foldoutViewKey;

            Toggle toggle = this.Q<Toggle>();
            _checkMark = toggle.Q<VisualElement>("unity-checkmark");

            VisualElement firstChild = toggle.Children().First();
            firstChild.style.width = Length.Percent(100);

            _dropdownBtn = UIToolkitUtils.MakeDropdownButtonUIToolkit(label);
            firstChild.Add(_dropdownBtn);

            _dropdownBtn.style.marginLeft = 0;
            _dropdownBtn.labelElement.style.marginLeft = 0;

            Type newType = value?.GetType();
            UnityObjectOverrideType = newType;

            if (labelGrayColor)
            {
                _dropdownBtn.labelElement.style.color = AbsRenderer.ReColor;
            }

            CheckRefresh(label, valueType, value, beforeSet, setterOrNull, targets);
            _init = true;

            _dropdownBtn.ButtonElement.clicked += OnDropdown;

            this.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue && !IsFullFilled)
                {
                    FillOrUpdateExpand(
                        _curValue, contentContainer, _beforeSet, _setterOrNull,
                        labelGrayColor, inHorizontalLayout,
                        _targets,
                        richTextTagProvider, foldoutViewKey);
                }
            });
        }

        private readonly bool _init;
        private Type _curType;
        private object _curValue;
        private Action<object> _beforeSet;
        private Action<object> _setterOrNull;
        private IReadOnlyList<object> _targets;


        // ReSharper disable once ParameterHidesMember
        public void CheckRefresh(string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            IReadOnlyList<object> targets)
        {
            _beforeSet = beforeSet;
            _setterOrNull = setterOrNull;
            _targets = targets;

            if (_dropdownBtn.label != label)
            {
                _dropdownBtn.label = label;
            }

            bool nullValue = value == null;
            // UIToolkitUtils.SetDisplayStyle(_checkMark, nullValue? DisplayStyle.None: DisplayStyle.Flex);
            Visibility checkMarkVisibility = nullValue ? Visibility.Hidden : Visibility.Visible;
            if (_checkMark.style.visibility != checkMarkVisibility)
            {
                _checkMark.style.visibility = checkMarkVisibility;
            }

            if (nullValue && this.value)
            {
                this.value = false;
            }

            Type instanceFieldType = RuntimeUtil.IsNull(value)
                ? null
                : value!.GetType();
            // Debug.Log($"{_curType?.Name}/{fieldType.Name}");

            if (!_init || _curType != instanceFieldType)
            {
                _curType = instanceFieldType;
                Type fieldType = valueType ?? value!.GetType();

                Type[] optionTypes = ReferencePickerAttributeDrawer
                    .GetTypesDerivedFrom(fieldType)
                    .ToArray();

                _dropdownBtn.ButtonLabelElement.text = value == null? "Null": $"{instanceFieldType!.Name} <color=grey>({instanceFieldType.Namespace})</color>";

                AdvancedDropdownList<Type> dropdownList = new AdvancedDropdownList<Type>();
                bool canBeNull = !fieldType.IsValueType;
                if(canBeNull)
                {
                    dropdownList.Add("[Null]", null);
                    if (optionTypes.Length > 0)
                    {
                        dropdownList.AddSeparator();
                    }
                }

                Dictionary<string, List<Type>> nameSpaceToTypes = new Dictionary<string, List<Type>>();
                foreach (Type type in optionTypes)
                {
                    string typeNamespace = type.Namespace;
                    if (string.IsNullOrEmpty(typeNamespace))
                    {
                        typeNamespace = "";
                    }
                    if (!nameSpaceToTypes.TryGetValue(typeNamespace, out List<Type> list))
                    {
                        nameSpaceToTypes[typeNamespace] = list = new List<Type>();
                    }
                    list.Add(type);
                    // string displayName = FormatTypeName(type);
                    // dropdownList.Add(new AdvancedDropdownList<Type>(displayName, type));
                }

                IOrderedEnumerable<string> nameSpaceToTypesSorted = nameSpaceToTypes.Keys.OrderBy(each => each);
                foreach (string @namespace in nameSpaceToTypesSorted)
                {
                    List<Type> types = nameSpaceToTypes[@namespace];
                    // types.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    AdvancedDropdownList<Type> namespaceTypes = new AdvancedDropdownList<Type>(@namespace == ""? "[No Namespace]": @namespace);
                    foreach (Type eachType in types)
                    {
                        namespaceTypes.Add(eachType.Name, eachType);
                    }
                    dropdownList.Add(namespaceTypes);
                }

                _cachedDropdownList = dropdownList;
            }

            if (!_init || _curValue != value)
            {
                _curValue = value;
                if (this.value)  // expanded
                {
                    IsFullFilled = true;
                    FillOrUpdateExpand(
                        value,
                        contentContainer,
                        beforeSet,
                        setterOrNull,
                        _labelGrayColor,
                        _inHorizontalLayout,
                        targets,
                        _richTextTagProvider,
                        _foldoutViewKey
                    );
                }
                else
                {
                    IsFullFilled = false;
                }
            }

        }

        private AdvancedDropdownList<Type> _cachedDropdownList;

        private void OnDropdown()
        {
            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = new []{_curType},
                DropdownListValue = _cachedDropdownList,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect dropBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(_dropdownBtn.ButtonElement.worldBound);
            UnityEditor.PopupWindow.Show(dropBound, new SaintsTreeDropdownUIToolkit(
                metaInfo,
                _dropdownBtn.ButtonElement.worldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    Type newType = (Type)curItem;
                    _beforeSet?.Invoke(_curValue);

                    if (newType == null)
                    {
                        _setterOrNull?.Invoke(null);
                        return null;
                    }

                    object obj;
                    try
                    {
                        obj = Activator.CreateInstance(newType);
                    }
                    catch (Exception e)
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogError(e);
#endif
                        obj = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(newType);
                    }

                    // Debug.Log($"Create {newType}: {obj}");
                    object copyObj = ReferencePickerAttributeDrawer.CopyObj(_curValue, obj);

                    _setterOrNull?.Invoke(copyObj);

                    return null;
                }
            ));
        }

        private static void FillOrUpdateExpand(object value,
            VisualElement fieldsBody,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            const BindingFlags bindAttrNormal = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

            bool valueIsNull = RuntimeUtil.IsNull(value);
            if (valueIsNull)
            {
                fieldsBody.Clear();
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            List<FieldInfo> fieldTargets = value.GetType().GetFields(bindAttrNormal).ToList();
            Dictionary<string, FieldInfo> backingToFieldInfo = fieldTargets
                .Where(each => each.Name.StartsWith("<") && each.Name.EndsWith(">k__BackingField"))
                .ToDictionary(each => each.Name);
            PropertyInfo[] propertyTargets = value.GetType().GetProperties(bindAttrNormal);
            foreach (PropertyInfo propertyInfo in propertyTargets)
            {
                string propName = propertyInfo.Name;
                string backingName = $"<{propName}>k__BackingField";
                if (backingToFieldInfo.TryGetValue(backingName, out FieldInfo dupInfo))
                {
                    fieldTargets.Remove(dupInfo);
                }
            }

            List<VisualElement> children = fieldsBody.Children().ToList();

            // Debug.Log($"fieldTargets={string.Join(",", fieldTargets.Select(each => each.Name))}");
            // Debug.Log($"propertyTargets={string.Join(",", propertyTargets.Select(each => each.Name))}");
            //
            // Debug.Log("Init generic type");
            foreach (FieldInfo fieldInfo in fieldTargets)
            {
                // string name = fieldInfo.Name;
                string subId = $"{foldoutViewKey}.{fieldInfo.Name}";

                if (AbsRenderer.SkipTypeDrawing(fieldInfo.FieldType))
                {
                    continue;
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
                Debug.Log($"render general field {name}/{fieldInfo.FieldType}/{fieldInfo.FieldType.Namespace}/{fieldInfo.FieldType.Name}");
#endif
                VisualElement oldItemElement = fieldsBody.Q<VisualElement>(className: subId);
                children.Remove(oldItemElement);
                string thisLabel = ObjectNames.NicifyVariableName(fieldInfo.Name);
                VisualElement result = null;

                object fieldValue = null;
                bool getValueSucceed = true;
                try
                {
                    fieldValue = fieldInfo.GetValue(value);
                }
                catch (Exception e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning($"field {subId}/{fieldInfo.FieldType} inside {value} gives error: {e}");
#endif
                    getValueSucceed = false;
                    string msg = e.InnerException?.Message ?? e.Message;
                    if (oldItemElement is ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField errorField)
                    {
                        errorField.SetErrorMessage(msg);
                    }
                    else
                    {
                        oldItemElement?.RemoveFromHierarchy();
                        oldItemElement = null;
                        ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField r = new ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField(
                            thisLabel,
                            new HelpBox(msg, HelpBoxMessageType.Error)
                        );
                        result = r;
                        if (inHorizontalLayout)
                        {
                            result.style.flexDirection = FlexDirection.Column;
                        }
                        else
                        {
                            result.AddToClassList(ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField.alignedFieldUssClassName);
                        }

                        if (labelGrayColor)
                        {
                            r.labelElement.style.color = AbsRenderer.ReColor;
                        }
                    }
                }

                // Debug.Log($"try render field {name}/{fieldInfo.FieldType} under {value}/{value?.GetType()}");

                if(getValueSucceed)
                {
                    if (oldItemElement is ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField)
                    {
                        oldItemElement.RemoveFromHierarchy();
                        oldItemElement = null;
                    }
                    result = UIToolkitEdit.UIToolkitValueEdit(
                        oldItemElement,
                        thisLabel,
                        fieldInfo.FieldType,
                        fieldValue,
                        // _ => beforeSet?.Invoke(value),
                        _ =>
                        {
                            // Debug.Log($"Before Set field {fieldInfo.Name}, invoke {value}");
                            beforeSet?.Invoke(value);
                        },
                        newValue =>
                        {
                            fieldInfo.SetValue(value, newValue);
                            setterOrNull?.Invoke(value);
                        },
                        labelGrayColor,
                        inHorizontalLayout,
                        ReflectCache.GetCustomAttributes(fieldInfo),
                        targets, richTextTagProvider,
                        subId).result;
                }
                // Debug.Log($"{name}: {result}: {fieldInfo.FieldType}");
                // ReSharper disable once InvertIf
                if(result != null)
                {
                    result.AddToClassList(subId);
                    fieldsBody.Add(result);
                }
            }

            foreach (PropertyInfo propertyInfo in propertyTargets)
            {
                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                if (propertyInfo.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                if (AbsRenderer.SkipTypeDrawing(propertyInfo.PropertyType))
                {
                    continue;
                }

                // string name = propertyInfo.Name;
                string subId = $"{foldoutViewKey}.{propertyInfo.Name}";
                VisualElement oldItemElement = fieldsBody.Q<VisualElement>(className: subId);
                // Debug.Log($"old element {subId} = {oldItemElement}");
                children.Remove(oldItemElement);
                string thisLabel = ObjectNames.NicifyVariableName(propertyInfo.Name);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
                Debug.Log(
                    $"render general property {propertyInfo.Name}/{propertyInfo.PropertyType} inside {value}");
#endif
                VisualElement result = null;
                object propertyValue = null;
                bool getValueSucceed = true;
                try
                {
                    propertyValue = propertyInfo.GetValue(value);
                }
                catch (Exception e)
                {
                    getValueSucceed = false;
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning($"property {subId}/{propertyInfo.PropertyType} inside {value} gives error: {e}");
                    Debug.LogWarning(e.InnerException ?? e);
#endif
                    string msg = e.InnerException?.Message ?? e.Message;
                    if (oldItemElement is ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField errorField)
                    {
                        errorField.SetErrorMessage(msg);
                    }
                    else
                    {
                        oldItemElement?.RemoveFromHierarchy();
                        oldItemElement = null;
                        ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField r = new ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField(
                            thisLabel,
                            new HelpBox(msg, HelpBoxMessageType.Error)
                        );
                        result = r;
                        if (inHorizontalLayout)
                        {
                            result.style.flexDirection = FlexDirection.Column;
                        }
                        else
                        {
                            result.AddToClassList(ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField.alignedFieldUssClassName);
                        }

                        if (labelGrayColor)
                        {
                            r.labelElement.style.color = AbsRenderer.ReColor;
                        }
                    }
                }

                if (getValueSucceed)
                {
                    if (oldItemElement is ShowInInspectorFieldRenderer.NativeFieldPropertyRendererErrorField)
                    {
                        oldItemElement.RemoveFromHierarchy();
                        oldItemElement = null;
                    }
                    result = UIToolkitEdit.UIToolkitValueEdit(
                        oldItemElement,
                        thisLabel,
                        propertyInfo.PropertyType,
                        propertyValue,
                        _ => beforeSet?.Invoke(value),
                        propertyInfo.CanWrite
                            ? newValue =>
                            {
                                propertyInfo.SetValue(value, newValue);
                                setterOrNull?.Invoke(value);
                            }
                            : null,
                        labelGrayColor,
                        inHorizontalLayout,
                        ReflectCache.GetCustomAttributes(propertyInfo),
                        targets,
                        richTextTagProvider,
                        subId).result;
                }

                // ReSharper disable once InvertIf
                if(result != null)
                {
                    result.AddToClassList(subId);
                    fieldsBody.Add(result);
                }
            }

            foreach (VisualElement noLongNeedChildren in children)
            {
                noLongNeedChildren.RemoveFromHierarchy();
            }
        }
    }
}
