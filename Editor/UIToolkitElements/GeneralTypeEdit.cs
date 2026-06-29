#if UNITY_2021_3_OR_NEWER
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
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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

        private class NullOrCreateButtonField : BaseField<object>
        {
            public readonly Button Button;

            private NullOrCreateButtonField(string label, Button visualInput) : base(label, visualInput)
            {
                Button = visualInput;
            }

            public static NullOrCreateButtonField Create(string label)
            {
                return new NullOrCreateButtonField(label, new Button
                {
                    text = " ",
                    style =
                    {
                        paddingLeft = 2,
                        paddingRight = 2,
                        unityTextAlign = TextAnchor.MiddleLeft,
                        flexGrow = 1,
                        flexShrink = 1,
                        textOverflow = TextOverflow.Ellipsis,
                    },
                    displayTooltipWhenElided = false,
                });
            }
        }


        private Type _unityObjectOverrideType;
        private bool _isFullFilled;

        private readonly UIToolkitUtils.DropdownButtonField _dropdownBtn;
        private readonly NullOrCreateButtonField _nullOrCreateButtonField;

        private readonly ObjectField _unityObjectField;
        private readonly Toggle _toggle;
        private readonly VisualElement _checkMark;

        private readonly bool _labelGrayColor;
        private readonly bool _inHorizontalLayout;
        private readonly IRichTextTagProvider _richTextTagProvider;
        private readonly string _foldoutViewKey;

        // private readonly bool _isUnityObjectOnly;
        private readonly IReadOnlyList<Type> _canHaveUnityTypes;

        // ReSharper disable once MemberCanBePrivate.Global
        public GeneralTypeEdit()
        {
        }

        public GeneralTypeEdit(string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            this.value = false;

            _labelGrayColor = labelGrayColor;
            _inHorizontalLayout = inHorizontalLayout;
            _richTextTagProvider = richTextTagProvider;
            _foldoutViewKey = foldoutViewKey;

            // _isUnityObjectOnly = valueType == typeof(Object) || valueType.IsSubclassOf(typeof(Object));

            _toggle = this.Q<Toggle>();
            _checkMark = _toggle.Q<VisualElement>("unity-checkmark");

            VisualElement firstChild = _toggle.Children().First();
            firstChild.style.width = Length.Percent(100);

            _dropdownBtn = UIToolkitUtils.MakeDropdownButtonUIToolkit(label);
            firstChild.Add(_dropdownBtn);
            _dropdownBtn.style.marginLeft = 0;
            _dropdownBtn.style.marginRight = 0;
            if (labelGrayColor)
            {
                _dropdownBtn.labelElement.style.color = AbsRenderer.ReColor;
            }

            _nullOrCreateButtonField = NullOrCreateButtonField.Create(label);
            firstChild.Add(_nullOrCreateButtonField);
            _nullOrCreateButtonField.style.display = DisplayStyle.None;
            _nullOrCreateButtonField.style.marginLeft = 0;
            _nullOrCreateButtonField.style.marginRight = 0;
            _nullOrCreateButtonField.style.flexGrow = 1;
            _nullOrCreateButtonField.style.flexShrink = 1;
            _nullOrCreateButtonField.labelElement.style.marginLeft = 0;
            _nullOrCreateButtonField.AddToClassList(NullOrCreateButtonField.alignedFieldUssClassName);
            if (labelGrayColor)
            {
                _nullOrCreateButtonField.labelElement.style.color = AbsRenderer.ReColor;
            }

            _canHaveUnityTypes = TypeCache.GetTypesDerivedFrom(valueType)
                .Prepend(valueType)
                .Where(each => !each.IsAbstract) // abstract classes
                .Where(each => !each.ContainsGenericParameters) // generic classes
                .Where(each => typeof(Object).IsAssignableFrom(each))
                .ToArray();

            Type newType = value?.GetType();
            if(_canHaveUnityTypes.Contains(newType))
            {
                _unityObjectOverrideType = newType;
            }

            Debug.Assert(_nullOrCreateButtonField != null);
            CheckRefresh(label, valueType, value, beforeSet, setterOrNull, targets);
            _init = true;

            _dropdownBtn.ButtonElement.clicked += OnDropdown;

            _nullOrCreateButtonField.Button.clicked += () =>
            {
                if (_curValue == null && _unityObjectOverrideType == null)
                {
                    if (_optionTypes.Length == 0)
                    {
                        return;
                    }

                    SetToType(_optionTypes[0]);
                    // _nullOrCreateButtonField.Button.text =
                    //     $"Set To Null ({_optionTypes[0].Name} <color=#808080>({_optionTypes[0].Namespace})</color>)";
                }
                else
                {
                    SetToType(null);
                    // _nullOrCreateButtonField.Button.text = $"Null (Create {_optionTypes[0].Name} <color=#808080>{_optionTypes[0].Namespace})</color>)";
                }
            };

            this.RegisterValueChangedCallback(evt =>
            {
                // Debug.Log($"evt.newValue={evt.newValue}, _isFullFilled={_isFullFilled}, curValue={_curValue}");
                if (evt.newValue && !_isFullFilled)
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

        private Type[] _optionTypes = Array.Empty<Type>();


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

            if (_nullOrCreateButtonField.label != label)
            {
                _nullOrCreateButtonField.label = label;
            }

            bool nullValue = value == null;
            bool allowExpand = !nullValue || _unityObjectOverrideType != null;
            if (allowExpand)
            {
                UIToolkitUtils.SetDisplayStyle(_checkMark, DisplayStyle.Flex);
                _dropdownBtn.labelElement.style.marginLeft = 0;
                _toggle.style.marginLeft = StyleKeyword.Null;
            }
            else
            {
                UIToolkitUtils.SetDisplayStyle(_checkMark, DisplayStyle.None);
                _dropdownBtn.labelElement.style.marginLeft = 3;
                _toggle.style.marginLeft = 0;
            }

            if (!nullValue && _canHaveUnityTypes.Contains(value.GetType()))
            {
                _unityObjectOverrideType = value.GetType();
            }

            bool onUnityType = _canHaveUnityTypes.Contains(_unityObjectOverrideType);

            Type instanceFieldType = RuntimeUtil.IsNull(value)
                ? null
                : value!.GetType();

            if (!_init || _curType != instanceFieldType || onUnityType || _optionTypes.Length <= 1)
            {
                Type fieldType = valueType ?? value!.GetType();
                if(_optionTypes.Length == 0)
                {
                    _optionTypes = ReferencePickerAttributeDrawer
                        .GetTypesDerivedFrom(fieldType)
                        .ToArray();
                }

                string newDropdownButtonLabel;
                string newCreateButtonLabel;
                string newCreateButtonTooltip;
                if (value == null)
                {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (onUnityType)
                    {
                        Debug.Assert(_unityObjectOverrideType != null, valueType);
                        // Don't use color=gray or Color=grey, and fuck Unity
                        newDropdownButtonLabel =
                            $"{_unityObjectOverrideType.Name} <color=#808080>({_unityObjectOverrideType.Namespace})</color>";
                        newCreateButtonLabel =
                            $"{_unityObjectOverrideType.Name} -> Null <color=#808080>({_unityObjectOverrideType.Namespace})</color>";
                        newCreateButtonTooltip = "Click to set to null";
                    }
                    else
                    {
                        newDropdownButtonLabel = "Null";

                        if (_optionTypes.Length == 0)
                        {
                            newCreateButtonLabel = "Null";
                            newCreateButtonTooltip = $"No implement for {fieldType.Name} <color=#808080>({fieldType.Namespace})</color>";
                        }
                        else
                        {
                            newCreateButtonLabel =
                                $"Null -> {_optionTypes[0].Name} <color=#808080>{_optionTypes[0].Namespace})</color>";
                            newCreateButtonTooltip = $"Click to create {_optionTypes[0].Name}";
                        }

                    }
                }
                else
                {
                    newDropdownButtonLabel =
                        $"{instanceFieldType!.Name} <color=#808080>({instanceFieldType.Namespace})</color>";
                    newCreateButtonLabel = $"{instanceFieldType!.Name} -> Null <color=#808080>({instanceFieldType.Namespace})</color>";
                    newCreateButtonTooltip = "Click to set to null";
                }

                if (_dropdownBtn.ButtonLabelElement.text != newDropdownButtonLabel)
                {
                    _dropdownBtn.ButtonLabelElement.text = newDropdownButtonLabel;
                }

                if (_nullOrCreateButtonField.Button.text != newCreateButtonLabel)
                {
                    _nullOrCreateButtonField.Button.text = newCreateButtonLabel;
                }
                if (_nullOrCreateButtonField.Button.tooltip != newCreateButtonTooltip)
                {
                    _nullOrCreateButtonField.Button.tooltip = newCreateButtonTooltip;
                }
            }

            if (!_init || _curType != instanceFieldType)
            {
                _curType = instanceFieldType;
                Type fieldType = valueType ?? value!.GetType();

                _optionTypes = ReferencePickerAttributeDrawer
                    .GetTypesDerivedFrom(fieldType)
                    .ToArray();

                Dropdown<Type> dropdownList = new Dropdown<Type>();
                bool canBeNull = !fieldType.IsValueType;
                if(canBeNull)
                {
                    dropdownList.Add("[Null]", null);
                    if (_optionTypes.Length > 0)
                    {
                        dropdownList.AddSeparator();
                    }
                }

                DisplayStyle dropdownBtnDisplay = DisplayStyle.Flex;
                if (_optionTypes.Length <= 1)
                {
                    dropdownBtnDisplay = DisplayStyle.None;
                    if (canBeNull)
                    {
                        _dropdownBtn.style.display = DisplayStyle.None;
                        if (_optionTypes.Length == 0) // no implement at all. might be interface
                        {
                            _nullOrCreateButtonField.style.display = DisplayStyle.Flex;
                            _nullOrCreateButtonField.SetEnabled(false);
                            _nullOrCreateButtonField.Button.text = $"Null ({fieldType.Name} <color=#808080>{fieldType.Namespace}</color>)";
                        }
                        else // has exactly one implement
                        {
                            _nullOrCreateButtonField.style.display = DisplayStyle.Flex;
                        }
                    }
                }
                UIToolkitUtils.SetDisplayStyle(_dropdownBtn.ButtonElement, dropdownBtnDisplay);

                Dictionary<string, List<Type>> nameSpaceToTypes = new Dictionary<string, List<Type>>();
                foreach (Type type in _optionTypes)
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
                    // dropdownList.Add(new Dropdown<Type>(displayName, type));
                }

                IOrderedEnumerable<string> nameSpaceToTypesSorted = nameSpaceToTypes.Keys.OrderBy(each => each);
                foreach (string @namespace in nameSpaceToTypesSorted)
                {
                    List<Type> types = nameSpaceToTypes[@namespace];
                    // types.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    Dropdown<Type> namespaceTypes = new Dropdown<Type>(@namespace == ""? "[No Namespace]": @namespace);
                    foreach (Type eachType in types)
                    {
                        namespaceTypes.Add(eachType.Name, eachType);
                    }
                    dropdownList.Add(namespaceTypes);
                }

                _cachedDropdownList = dropdownList;
            }

            // Debug.Log($"{_curType?.Name}/{fieldType.Name}");

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
            Debug.Log($"_curValue={_curValue}, value={value}, update={_curValue != value}");
#endif

            // When expanded, the sub-fields might need update. So also update if it's expanded (this.value==true)
            if (!_init || _curValue != value || this.value)
            {
                _curValue = value;
                if (this.value)  // expanded
                {
                    _isFullFilled = true;
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
                    _isFullFilled = false;
                }
            }

        }

        private Dropdown<Type> _cachedDropdownList;

        private void OnDropdown()
        {
            Type selectedType = _curType;
            if (_curValue == null && _canHaveUnityTypes.Contains(_unityObjectOverrideType))
            {
                selectedType = _unityObjectOverrideType;
            }
            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = new []{selectedType},
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
                    SetToType(newType);
                    return null;
                }
            ));
        }

        private void SetToType(Type newType)
        {
            _beforeSet?.Invoke(_curValue);

            bool isUnityObjectType = _canHaveUnityTypes.Contains(newType);
            // Debug.Log($"isUnityObjectType={isUnityObjectType}/{newType}");
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (isUnityObjectType)
            {
                _unityObjectOverrideType = newType;
                // Debug.Log($"_unityObjectOverrideType={_unityObjectOverrideType}");
            }
            else
            {
                _unityObjectOverrideType = null;
            }

            if (newType == null || isUnityObjectType)
            {
                _setterOrNull?.Invoke(null);
                // ReSharper disable once InvertIf
                if(newType != null)
                {
                    value = true;
                    UpdateOrAddUnityObjectField(contentContainer, null);
                }
                return;
            }

            object obj;
            try
            {
                obj = Activator.CreateInstance(newType);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(e);
#endif
                obj = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(newType);
            }

            // Debug.Log($"Create {newType}: {obj}");
            object copyObj = ReferencePickerAttributeDrawer.CopyObj(_curValue, obj);

            _setterOrNull?.Invoke(copyObj);
            value = true;
        }

        // ReSharper disable once ParameterHidesMember
        private void FillOrUpdateExpand(object value,
            VisualElement fieldsBody,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            const BindingFlags bindAttrNormal = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

            bool valueIsNull = RuntimeUtil.IsNull(value);
            if (valueIsNull)
            {
                if (_canHaveUnityTypes.Count > 0 && _unityObjectOverrideType != null)
                {
                    UpdateOrAddUnityObjectField(fieldsBody, null);
                }
                else
                {
                    fieldsBody.Clear();
                }
                return;
            }

            if (_canHaveUnityTypes.Contains(value.GetType()))
            {
                UpdateOrAddUnityObjectField(fieldsBody, (Object) value);
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
                    oldItemElement?.RemoveFromHierarchy();
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

        private void UpdateOrAddUnityObjectField(VisualElement fieldsBody, Object uObj)
        {
            string className = $"{_foldoutViewKey}[object]";
            ObjectField field = fieldsBody.Q<ObjectField>(className: className);
            if (field == null)
            {
                // Debug.Log($"not found {className} for Unity object {UnityObjectOverrideType?.Name}, create");
                ObjectField newCreated = new ObjectField
                {
                    objectType = _unityObjectOverrideType,
                    allowSceneObjects = true,
                };
                newCreated.AddToClassList(className);
                fieldsBody.Add(newCreated);

                newCreated.RegisterValueChangedCallback(evt =>
                {
                    Object newValue = evt.newValue;
                    _beforeSet?.Invoke(_curValue);
                    _setterOrNull?.Invoke(newValue);
                });
                field = newCreated;
            }
            else
            {
                if (field.objectType != _unityObjectOverrideType)
                {
                    // Debug.Log($"update unityType from {field.objectType} to {UnityObjectOverrideType}");
                    field.objectType = _unityObjectOverrideType;
                }
            }

            if (!ReferenceEquals(field.value, uObj))
            {
                field.SetValueWithoutNotify(uObj);
            }

            foreach (VisualElement children in fieldsBody.Children().ToArray())
            {
                if (!ReferenceEquals(children, field))
                {
                    children.RemoveFromHierarchy();
                }
            }
        }
    }
}
#endif
