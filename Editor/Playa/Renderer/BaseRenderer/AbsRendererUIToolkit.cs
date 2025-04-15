#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ReferencePicker;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public abstract partial class AbsRenderer
    {
        private const string ClassSaintsFieldPlaya = "saintsfield-playa";
        private const string ClassSaintsFieldEditingDisabled = "saintsfield-editing-disabled";
        public const string ClassSaintsFieldPlayaContainer = ClassSaintsFieldPlaya + "-container";

        private VisualElement _rootElement;

        public virtual VisualElement CreateVisualElement()
        {
            int flexGrow;
            if (InDirectHorizontalLayout)
            {
                flexGrow = 1;
            }
            else
            {
                flexGrow = InAnyHorizontalLayout ? 0 : 1;
            }

            VisualElement root = new VisualElement
            {
                style =
                {
                    // flexGrow = 1,
                    // flexGrow = InAnyHorizontalLayout? 0: 1,
                    flexGrow = flexGrow,
                    width = new StyleLength(Length.Percent(100)),
                },
                name = ToString(),
            };
            root.AddToClassList(ClassSaintsFieldPlaya);
            bool hasAnyChildren = false;

            (VisualElement aboveTarget, bool aboveNeedUpdate) = CreateAboveUIToolkit();
            if (aboveTarget != null)
            {
                root.Add(aboveTarget);
                hasAnyChildren = true;
            }
            (VisualElement target, bool targetNeedUpdate) = CreateTargetUIToolkit(root);
            if (target != null)
            {
                VisualElement targetContainer = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 0,

                        width = new StyleLength(Length.Percent(100)),
                    },
                };
                targetContainer.AddToClassList(ClassSaintsFieldPlayaContainer);
                targetContainer.Add(target);
                root.Add(targetContainer);
                hasAnyChildren = true;
            }
            (VisualElement belowTarget, bool belowNeedUpdate) = CreateBelowUIToolkit();
            if (belowTarget != null)
            {
                root.Add(belowTarget);
                hasAnyChildren = true;
            }

            bool anyNeedUpdate = aboveNeedUpdate || targetNeedUpdate || belowNeedUpdate;
            if (anyNeedUpdate)
            {
                root.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    // OnUpdateUIToolKit();
                    root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement));
                    root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement)).Every(100);
                });
            }
            if(anyNeedUpdate || hasAnyChildren)
            {
                return _rootElement = root;
            }

            return null;
        }

        protected virtual (VisualElement target, bool needUpdate) CreateAboveUIToolkit()
        {
            return (null, false);
        }

        protected abstract (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container);

        protected virtual (VisualElement target, bool needUpdate) CreateBelowUIToolkit()
        {
            return (null, false);
        }

        private static void MergeIntoGroup(Dictionary<string, VisualElement> groupElements, string groupBy, VisualElement root, VisualElement child)
        {
            if (string.IsNullOrEmpty(groupBy))
            {
                root.Add(child);
                return;
            }

            bool exists = groupElements.TryGetValue(groupBy, out VisualElement groupElement);
            if (!exists)
            {
                groupElement = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                    }
                };
                groupElement.AddToClassList($"{ClassSaintsFieldPlaya}-group-{groupBy}");
                groupElements.Add(groupBy, groupElement);
                root.Add(groupElement);
            }

            groupElement.Add(child);
        }

        private class InfoBoxUserData
        {
            public string XmlContent;
            public EMessageType MessageType;

            public PlayaInfoBoxAttribute InfoBoxAttribute;
            public SaintsFieldWithInfo FieldWithInfo;
            public RichTextDrawer RichTextDrawer;
        }


        protected virtual PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            return UpdatePreCheckUIToolkit();
        }

        protected PreCheckResult HelperOnUpdateUIToolKitRawBase()
        {
            return UpdatePreCheckUIToolkit();
        }

        protected PreCheckResult UpdatePreCheckUIToolkit()
        {
            return UpdatePreCheckUIToolkitInternal(FieldWithInfo, _rootElement);
        }

        // TODO: paging & searching
        private PreCheckResult UpdatePreCheckUIToolkitInternal(SaintsFieldWithInfo fieldWithInfo, VisualElement result)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(fieldWithInfo, false);
            if(result.enabledSelf != !preCheckResult.IsDisabled)
            {
                result.SetEnabled(!preCheckResult.IsDisabled);
            }

            bool isShown = result.style.display != DisplayStyle.None;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PLAYA_IS_SHOWN
            Debug.Log($"{fieldWithInfo} {result.name} isShown={isShown}, preCheckIsShown={preCheckResult.IsShown}");
#endif

            if(isShown != preCheckResult.IsShown)
            {
                result.style.display = preCheckResult.IsShown ? DisplayStyle.Flex : DisplayStyle.None;
            }

            return preCheckResult;
        }

        private static StyleSheet _nullUss;

        private static VisualElement WrapVisualElement(VisualElement visualElement)
        {
            visualElement.SetEnabled(false);
            visualElement.AddToClassList(ClassSaintsFieldEditingDisabled);
            // visualElement.AddToClassList("unity-base-field__aligned");
            visualElement.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);
            return visualElement;
        }

        private class LabelButtonField : BaseField<object>
        {
            public LabelButtonField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private enum UIToolkitValueEditPayloadState
        {
            None,
            FieldObject,
            GenericType,
        }

        private class UIToolkitValueEditPayload
        {
            public Type UnityObjectOverrideType;
            public UIToolkitValueEditPayloadState State;
        }

        // before set: useful for struct editing that C# will messup and change the value of the reference you have
        protected static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {

            // Debug.Log(valueType);
            // if (RuntimeUtil.IsNull(value))
            // {
            //     return null;
            // }

            Color reColor = EColor.EditorSeparator.GetColor();

            if (valueType == typeof(bool) || value is bool)
            {
                if (oldElement is Toggle oldToggle)
                {
                    oldToggle.SetValueWithoutNotify(Convert.ToBoolean(value));
                    return null;
                }

                Toggle element = new Toggle(label)
                {
                    value = (bool)value,
                };
                if (inHorizontalLayout)
                {
                    // element.style.flexDirection = FlexDirection.RowReverse;
                    Label toggleLabel = element.Q<Label>();
                    if(toggleLabel != null)
                    {
                        toggleLabel.style.minWidth = 0;
                    }
                }
                else
                {
                    element.AddToClassList(Toggle.alignedFieldUssClassName);
                }
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(sbyte) || value is sbyte)
            {
                if (oldElement is IntegerField integerField)
                {
                    integerField.SetValueWithoutNotify((sbyte)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (sbyte)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(IntegerField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        sbyte newValue = (sbyte)evt.newValue;
                        beforeSet?.Invoke(value);
                        setterOrNull(newValue);
                        if (newValue != evt.newValue)
                        {
                            element.SetValueWithoutNotify(newValue);
                        }
                    });
                }

                return element;
            }
            if (valueType == typeof(byte) || value is byte)
            {
                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((byte)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (byte)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(IntegerField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        byte newValue = (byte)evt.newValue;
                        beforeSet?.Invoke(value);
                        setterOrNull(newValue);
                        if (newValue != evt.newValue)
                        {
                            element.SetValueWithoutNotify(newValue);
                        }
                    });
                }

                return element;
            }
            if (valueType == typeof(short) || value is short)
            {
                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((short)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (short)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(IntegerField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        short newValue = (short)evt.newValue;
                        beforeSet?.Invoke(value);
                        setterOrNull(newValue);
                        if (newValue != evt.newValue)
                        {
                            element.SetValueWithoutNotify(newValue);
                        }
                    });
                }
                return element;
            }
            if (valueType == typeof(ushort) || value is ushort)
            {
                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((ushort)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (ushort)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(IntegerField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        ushort newValue = (ushort)evt.newValue;
                        beforeSet?.Invoke(value);
                        setterOrNull(newValue);
                        if (newValue != evt.newValue)
                        {
                            element.SetValueWithoutNotify(newValue);
                        }
                    });
                }

                return element;
            }
            if (valueType == typeof(int) || value is int)
            {
                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((int)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (int)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                // element.labelElement.style.borderRightColor = EColor.EditorEmphasized.GetColor();
                // element.labelElement.style.borderRightWidth = 1;
                // element.labelElement.style.color = EColor.EditorEmphasized.GetColor();
                element.labelElement.style.color = EColor.EditorSeparator.GetColor();
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(IntegerField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
                        Debug.Log($"Invoke old value {value}");
#endif
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(uint) || value is uint)
            {
#if UNITY_2022_3_OR_NEWER
                if (oldElement is UnsignedIntegerField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify((uint)value);
                    return null;
                }

                UnsignedIntegerField element = new UnsignedIntegerField(label)
                {
                    value = (uint)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(UnsignedIntegerField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        uint newValue = evt.newValue;
                        beforeSet?.Invoke(value);
                        setterOrNull(newValue);
                    });
                }
#else
                if (oldElement is IntegerField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify((int)(uint)value);
                    return null;
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (int)(uint)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(IntegerField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        uint newValue = (uint)evt.newValue;
                        beforeSet?.Invoke(value);
                        setterOrNull(newValue);
                    });
                }
#endif

                return element;
            }
            if (valueType == typeof(long) || value is long)
            {
                if (oldElement is LongField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify((long)value);
                    return null;
                }

                LongField element = new LongField(label)
                {
                    value = (long)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(LongField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(ulong) || value is ulong)
            {
#if UNITY_2022_3_OR_NEWER
                ulong ulongRawValue = (ulong)value;
                if (oldElement is UnsignedLongField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify(ulongRawValue);
                    return null;
                }

                UnsignedLongField element = new UnsignedLongField(label)
                {
                    value = ulongRawValue,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(UnsignedLongField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        ulong useNewValue = evt.newValue;
                        beforeSet?.Invoke(value);
                        setterOrNull(useNewValue);
                    });
                }
#else
                long ulongRawValue = (long)(ulong)value;
                if (oldElement is LongField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify(ulongRawValue);
                    return null;
                }

                LongField element = new LongField(label)
                {
                    value = ulongRawValue,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(LongField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        ulong useNewValue = (ulong)evt.newValue;
                        beforeSet?.Invoke(value);
                        setterOrNull(useNewValue);
                    });
                }
#endif

                return element;
            }
            if (valueType == typeof(float) || value is float)
            {
                if (oldElement is FloatField oldFloatField)
                {
                    oldFloatField.SetValueWithoutNotify((float)value);
                    return null;
                }

                FloatField element = new FloatField(label)
                {
                    value = (float)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(FloatField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(double) || value is double)
            {
                if (oldElement is DoubleField oldDoubleField)
                {
                    oldDoubleField.SetValueWithoutNotify((double)value);
                    return null;
                }

                DoubleField element = new DoubleField(label)
                {
                    value = (double)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(DoubleField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(string) || value is string)
            {
                if (oldElement is TextField oldTextField)
                {
                    oldTextField.SetValueWithoutNotify((string)value);
                    return null;
                }

                TextField element = new TextField(label)
                {
                    value = (string)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }

                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(TextField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(char) || value is char)
            {
                if (oldElement is TextField oldTextField)
                {
                    oldTextField.maxLength = 1;
                    oldTextField.SetValueWithoutNotify($"{value}");
                    return null;
                }

                TextField element = new TextField(label)
                {
                    value = $"{value}",
                    maxLength = 1,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }

                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                    // a bug in Unity 6000.0.41f1
                    // TextInput but not such element, at all...
                    TextElement te = element.Q<VisualElement>("unity-text-input")?.Q<TextElement>();
                    if (te != null)
                    {
                        te.style.minHeight = 15;
                    }
                }
                else
                {
                    element.AddToClassList(TextField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        string newValue = evt.newValue;

                        if (string.IsNullOrEmpty(newValue))
                        {
                            return;
                        }

                        if (newValue.Length > 1)
                        {
                            // ReSharper disable once ReplaceSubstringWithRangeIndexer
                            newValue = newValue.Substring(0, 1);
                            element.SetValueWithoutNotify(newValue);
                        }

                        beforeSet?.Invoke(value);

                        char newChar = newValue[0];
                        setterOrNull(newChar);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector2) || value is Vector2)
            {
                if (oldElement is Vector2Field oldVector2Field)
                {
                    oldVector2Field.SetValueWithoutNotify((Vector2)value);
                    return null;
                }

                Vector2Field element = new Vector2Field(label)
                {
                    value = (Vector2)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    // element.style.flexDirection = FlexDirection.Column;
                    // element.style.flexWrap = Wrap.Wrap;
                    Label elementLabel = element.Q<Label>();
                    if (elementLabel != null)
                    {
                        elementLabel.style.minWidth = 0;
                        elementLabel.style.borderRightWidth = 1;
                        elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                    }
                }
                else
                {
                    element.AddToClassList(Vector2Field.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector3) || value is Vector3)
            {
                if (oldElement is Vector3Field oldVector3Field)
                {
                    oldVector3Field.SetValueWithoutNotify((Vector3)value);
                    return null;
                }

                Vector3Field element = new Vector3Field(label)
                {
                    value = (Vector3)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    // element.style.flexDirection = FlexDirection.Column;
                    // element.style.flexWrap = Wrap.Wrap;
                    Label elementLabel = element.Q<Label>();
                    if (elementLabel != null)
                    {
                        elementLabel.style.minWidth = 0;
                        elementLabel.style.borderRightWidth = 1;
                        elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                    }
                }
                else
                {
                    element.AddToClassList(Vector3Field.alignedFieldUssClassName);
                }

                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector4) || value is Vector4)
            {
                if (oldElement is Vector4Field oldVector4Field)
                {
                    oldVector4Field.SetValueWithoutNotify((Vector4)value);
                    return null;
                }

                Vector4Field element = new Vector4Field(label)
                {
                    value = (Vector4)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    // element.style.flexDirection = FlexDirection.Column;
                    // element.style.flexWrap = Wrap.Wrap;
                    Label elementLabel = element.Q<Label>();
                    if (elementLabel != null)
                    {
                        elementLabel.style.minWidth = 0;
                        elementLabel.style.borderRightWidth = 1;
                        elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                    }
                }
                else
                {
                    element.AddToClassList(Vector4Field.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector2Int) || value is Vector2Int)
            {
                if (oldElement is Vector2IntField oldVector2IntField)
                {
                    oldVector2IntField.SetValueWithoutNotify((Vector2Int)value);
                    return null;
                }

                Vector2IntField element = new Vector2IntField(label)
                {
                    value = (Vector2Int)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    // element.style.flexDirection = FlexDirection.Column;
                    // element.style.flexWrap = Wrap.Wrap;
                    Label elementLabel = element.Q<Label>();
                    if (elementLabel != null)
                    {
                        elementLabel.style.minWidth = 0;
                        elementLabel.style.borderRightWidth = 1;
                        elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                    }
                }
                else
                {
                    element.AddToClassList(Vector2IntField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Vector3Int) || value is Vector3Int)
            {
                if (oldElement is Vector3IntField oldVector3IntField)
                {
                    oldVector3IntField.SetValueWithoutNotify((Vector3Int)value);
                    return null;
                }

                Vector3IntField element = new Vector3IntField(label)
                {
                    value = (Vector3Int)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    // element.style.flexDirection = FlexDirection.Column;
                    // element.style.flexWrap = Wrap.Wrap;
                    Label elementLabel = element.Q<Label>();
                    if (elementLabel != null)
                    {
                        elementLabel.style.minWidth = 0;
                        elementLabel.style.borderRightWidth = 1;
                        elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                    }
                }
                else
                {
                    element.AddToClassList(Vector3IntField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    beforeSet?.Invoke(value);
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Color) || value is Color)
            {
                if (oldElement is ColorField oldColorField)
                {
                    oldColorField.SetValueWithoutNotify((Color)value);
                    return null;
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                Debug.Log($"Create color field for {label}");
#endif

                ColorField element = new ColorField(label)
                {
                    value = (Color)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(ColorField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                        Debug.Log($"Set Color {evt.newValue}");
#endif
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Bounds) || value is Bounds)
            {
                if (oldElement is BoundsField oldBoundsField)
                {
                    oldBoundsField.SetValueWithoutNotify((Bounds)value);
                    return null;
                }

                BoundsField element = new BoundsField(label)
                {
                    value = (Bounds)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    // element.style.flexDirection = FlexDirection.Column;
                    // element.style.flexWrap = Wrap.Wrap;
                    // Label elementLabel = element.Q<Label>();
                    // if (elementLabel != null)
                    // {
                    //     elementLabel.style.minWidth = 0;
                    //     elementLabel.style.borderRightWidth = 1;
                    //     elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                    // }
                }
                else
                {
                    element.AddToClassList(BoundsField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(Rect) || value is Rect)
            {
                if (oldElement is RectField oldRectField)
                {
                    oldRectField.SetValueWithoutNotify((Rect)value);
                    return null;
                }

                RectField element = new RectField(label)
                {
                    value = (Rect)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    // element.style.flexDirection = FlexDirection.Column;
                    // element.style.flexWrap = Wrap.Wrap;
                    Label elementLabel = element.Q<Label>();
                    if (elementLabel != null)
                    {
                        elementLabel.style.minWidth = 0;
                        elementLabel.style.borderRightWidth = 1;
                        elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                    }
                }
                else
                {
                    element.AddToClassList(RectField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType == typeof(RectInt) || value is RectInt)
            {
                if (oldElement is RectIntField oldRectIntField)
                {
                    oldRectIntField.SetValueWithoutNotify((RectInt)value);
                    return null;
                }

                RectIntField element = new RectIntField(label)
                {
                    value = (RectInt)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                if (inHorizontalLayout)
                {
                    // element.style.flexDirection = FlexDirection.Column;
                    // element.style.flexWrap = Wrap.Wrap;
                    Label elementLabel = element.Q<Label>();
                    if (elementLabel != null)
                    {
                        elementLabel.style.minWidth = 0;
                        elementLabel.style.borderRightWidth = 1;
                        elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                    }
                }
                else
                {
                    element.AddToClassList(RectIntField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (valueType.BaseType == typeof(Enum) || value is Enum)
            {
                if (oldElement is EnumField oldEnumField)
                {
                    oldEnumField.SetValueWithoutNotify((Enum)value);
                    return null;
                }

                EnumField element = new EnumField(label, (Enum)value);
                if (labelGrayColor)
                {
                    element.labelElement.style.color = reColor;
                }
                // ReSharper disable once PossibleNullReferenceException
                typeof(EnumField).GetField("m_EnumType", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(element, valueType);
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                    // element.style.flexWrap = Wrap.Wrap;
                    // Label elementLabel = element.Q<Label>();
                    // if (elementLabel != null)
                    // {
                    //     elementLabel.style.minWidth = 0;
                    //     elementLabel.style.borderRightWidth = 1;
                    //     elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                    // }
                }
                else
                {
                    element.AddToClassList(EnumField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return element;
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(valueType) || value is UnityEngine.Object)
            {
                return UIToolkitObjectFieldEdit(oldElement, label, valueType, (UnityEngine.Object)value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout);
            }

            bool valueIsNull = RuntimeUtil.IsNull(value);

            IEnumerable<Type> genTypes = valueType.GetInterfaces()
                .Where(each => each.IsGenericType)
                // .Select(each => each.GetGenericTypeDefinition())
                ;
            if (valueType.IsGenericType)
            {
                genTypes = genTypes.Prepend(valueType);
            }

            Type dictionaryInterface = typeof(IDictionary<,>);
            Type readonlyDictionaryInterface = typeof(IReadOnlyDictionary<,>);
            // ReSharper disable once NotAccessedVariable
            Type[] dictionaryArgTypes = null;
            bool isNormalDictionary = false;
            bool isReadonlyDictionary = false;

            // IDictionary;
            // IReadOnlyDictionary;
            if(!valueIsNull)
            {
                foreach (Type normType in genTypes)
                {
                    Type genType = normType.GetGenericTypeDefinition();
                    // Debug.Log(genType);
                    if (genType == dictionaryInterface)
                    {
                        isNormalDictionary = true;
                        // ReSharper disable once RedundantAssignment
                        dictionaryArgTypes = normType.GetGenericArguments();
                        break;
                    }

                    if (genType == readonlyDictionaryInterface)
                    {
                        isReadonlyDictionary = true;
                        // ReSharper disable once RedundantAssignment
                        dictionaryArgTypes = normType.GetGenericArguments();
                        // break;
                    }
                }
            }

            if(isNormalDictionary || isReadonlyDictionary)
            {
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK
                bool isReadOnly = !isNormalDictionary;
                // Debug.Log($"MakeDictionaryView isReadOnly={isReadOnly}/{oldElement}");
                return MakeDictionaryView(oldElement as Foldout, label, valueType, value, isReadOnly, dictionaryArgTypes[0], dictionaryArgTypes[1], beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout);
#else  // WTF Unity, backport it!
                // ReSharper disable once AssignNullToNotNullAttribute
                object[] kvPairs = (value as IEnumerable).Cast<object>().ToArray();

                Foldout foldout = new Foldout
                {
                    text = $"{label} <color=#808080ff>(Dictionary x{kvPairs.Length})</color>",
                };
                foldout.AddToClassList("saintsfield-dictionary");

                const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;


                foreach ((object kvPair, int index) in kvPairs.WithIndex())
                {
                    string kvContainerName = $"kv-{index}";
                    VisualElement kvContainerOldElement = oldElement?.Q<VisualElement>(name: kvContainerName);
                    bool hasOldContainer = kvContainerOldElement != null;
                    VisualElement kvContainer = hasOldContainer ? kvContainerOldElement : null;

                    Type kvPairType = kvPair.GetType();
                    PropertyInfo keyProp = kvPairType.GetProperty("Key", bindAttr);
                    if (keyProp == null)
                    {
                        foldout.Add(new HelpBox($"Failed to obtain key on element {index}: {kvPair}", HelpBoxMessageType.Error));
                        continue;
                    }
                    PropertyInfo valueProp = kvPairType.GetProperty("Value", bindAttr);
                    if (valueProp == null)
                    {
                        foldout.Add(new HelpBox($"Failed to obtain value on element {index}: {kvPair}", HelpBoxMessageType.Error));
                        continue;
                    }

                    object dictKey = keyProp.GetValue(kvPair);
                    string keyElemName = $"key-{index}";
                    VisualElement oldKeyElement = kvContainer?.Q<VisualElement>(name: keyElemName);
                    VisualElement newKeyElement = UIToolkitValueEdit(
                        oldKeyElement,
                        $"{dictKey} <color=#808080ff>(Key {index})</color>",
                        keyProp.PropertyType,
                        dictKey,
                        null,
                        null,
                        false,
                        inHorizontalLayout
                    );
                    if (newKeyElement != null)
                    {
                        newKeyElement.name = keyElemName;
                        foldout.Add(newKeyElement);
                    }

                    string valueContainerName = $"value-container-{index}";
                    VisualElement oldValueContainer = kvContainer?.Q<VisualElement>(name: valueContainerName);
                    object dictValue = valueProp.GetValue(kvPair);
                    VisualElement newValueContainer = UIToolkitValueEdit(
                        oldValueContainer,
                        $"{dictValue} <color=#808080ff>(Value {index})</color>",
                        valueProp.PropertyType,
                        dictValue,
                        null,
                        null,
                        false,
                        inHorizontalLayout
                    );
                    if (newValueContainer != null)
                    {
                        newValueContainer.name = valueContainerName;
                        newValueContainer.style.paddingLeft = SaintsPropertyDrawer.IndentWidth;
                        foldout.Add(newValueContainer);
                    }
                    // VisualElement valueContainer = new VisualElement
                    // {
                    //     style =
                    //     {
                    //         paddingLeft = SaintsPropertyDrawer.IndentWidth,
                    //     },
                    // };
                    // valueContainer.Add(UIToolkitLayout(dictValue, $"{dictValue} <color=#808080ff>(Value {index})</color>"));
                    // foldout.Add(valueContainer);
                }

                return foldout;
#endif
            }
            if (value is IEnumerable enumerableValue)
            {
                // Debug.Log($"oldElement={oldElement}, {oldElement is Foldout}");
                return MakeListView(oldElement as Foldout, label, valueType, enumerableValue, enumerableValue.Cast<object>().ToArray(), beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout);
            }

            // Debug.Log(valueType);

            if (valueType.BaseType == typeof(TypeInfo))  // generic type?
            {
                // EditorGUILayout.TextField(label, value.ToString());
                return WrapVisualElement(new TextField(label)
                {
                    value = value.ToString(),
                });
            }

            if (valueIsNull)
            {
                if (valueType.IsArray || typeof(IList).IsAssignableFrom(valueType))
                {
                    LabelButtonField labelButtonField = new LabelButtonField(label, new Button(() =>
                    {
                        beforeSet?.Invoke(value);
                        object result = valueType.IsArray ? Array.CreateInstance(ReflectUtils.GetElementType(valueType), 0) : Activator.CreateInstance(valueType);
                        setterOrNull(result);
                        // return;
                        // setterOrNull(Activator.CreateInstance(valueType));
                    })
                    {
                        text = $"null (Click to Create)",
                        tooltip = "Click to Create",
                        style =
                        {
                            flexGrow = 1,
                            unityTextAlign = TextAnchor.MiddleLeft,
                        },
                    });
                    if (labelGrayColor)
                    {
                        labelButtonField.labelElement.style.color = reColor;
                    }
                    labelButtonField.AddToClassList(LabelButtonField.alignedFieldUssClassName);
                    return labelButtonField;
                }

                if (setterOrNull == null)
                {
                    TextField textField = new TextField(label)
                    {
                        value = "null",
                        pickingMode = PickingMode.Ignore,
                    };
                    if (labelGrayColor)
                    {
                        textField.labelElement.style.color = reColor;
                    }

                    if(_nullUss == null)
                    {
                        _nullUss = Util.LoadResource<StyleSheet>("UIToolkit/UnityTextInputElementWarning.uss");
                    }
                    textField.styleSheets.Add(_nullUss);

                    return WrapVisualElement(textField);
                }

                // UIToolkitUtils.DropdownButtonField dropdownButton = MakeTypeDropdown(label, valueType, null,
                //     selectedType =>
                //     {
                //
                //     });
                // dropdownButton.ButtonElement.text = "null";
                // return dropdownButton;
            }

            const string objFieldName = "saintsfield-objectfield";

            // Debug.Log(ReflectUtils.GetMostBaseType(valueType));
            const BindingFlags bindAttrNormal = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            Foldout genFoldout = oldElement as Foldout;
            if (genFoldout != null && !genFoldout.ClassListContains("saintsfield-general"))
            {
                genFoldout = null;
            }

            bool useOld = genFoldout != null;
            if (!useOld)
            {
                genFoldout = new Foldout
                {
                    text = label,
                    style =
                    {
                        position = Position.Relative,
                    },
                    userData = new UIToolkitValueEditPayload
                    {
                        UnityObjectOverrideType = value?.GetType(),
                    },
                };
                if (labelGrayColor)
                {
                    genFoldout.style.color = reColor;
                }
                genFoldout.AddToClassList("saintsfield-general");

                VisualElement fieldsBodyNew = new VisualElement
                {
                    name = "saintsfield-edit-fields",
                };

                genFoldout.Add(MakeTypeDropdown("", valueType, value, newType =>
                {
                    UIToolkitValueEditPayload payload = (UIToolkitValueEditPayload)genFoldout.userData;
                    Type preType = payload.UnityObjectOverrideType;
                    payload.UnityObjectOverrideType = newType;

                    if (payload.State == UIToolkitValueEditPayloadState.FieldObject && newType != null &&
                        typeof(UnityEngine.Object).IsAssignableFrom(newType))
                    {
                        // string objFieldName = $"saintsfield-objectfield";
                        if (preType != newType)
                        {
                            // ObjectField preField = fieldsBodyNew.Q<ObjectField>(name: objFieldName);
                            VisualElement fieldsBody = genFoldout.Q<VisualElement>(name: "saintsfield-edit-fields");
                            // Debug.Log($"swap {preType} -> {newType}: {fieldsBody.childCount}");
                            fieldsBody.Clear();

                            bool canConvert = value == null || newType.IsAssignableFrom(value.GetType());

                            ObjectField objFieldResult;
                            if(canConvert)
                            {
                                objFieldResult =
                                    UIToolkitObjectFieldEdit(null, label, newType, (UnityEngine.Object) value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout);
                            }
                            else
                            {
                                objFieldResult =
                                    UIToolkitObjectFieldEdit(null, label, newType, null, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout);
                                beforeSet?.Invoke(value);
                                setterOrNull?.Invoke(null);
                            }
                            objFieldResult.name = objFieldName;
                            fieldsBody.Add(objFieldResult);
                            objFieldResult.label = "";
                        }

                        return;
                    }

                    // Debug.Log(($"Override new type: {newType}"));
                    if (newType == null)
                    {
                        setterOrNull?.Invoke(null);
                        if (payload.State != UIToolkitValueEditPayloadState.None)
                        {
                            payload.State = UIToolkitValueEditPayloadState.None;
                            fieldsBodyNew.Clear();
                        }
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(newType))
                    {
                        setterOrNull?.Invoke(null);
                        // the objectoverride will handle the rest
                        if (payload.State != UIToolkitValueEditPayloadState.FieldObject)
                        {
                            fieldsBodyNew.Clear();
                        }
                        else
                        {
                            // string objFieldName = $"saintsfield-objectfield-{preType?.Name}";
                            if (preType != newType)
                            {
                                ObjectField preField = fieldsBodyNew.Q<ObjectField>(name: objFieldName);
                                // Debug.Log($"swap {preType} -> {newType}: {preField}");
                                if (preField != null)
                                {
                                    preField.SetValueWithoutNotify(null);
                                    preField.objectType = newType;
                                }
                            }
                        }

                    }
                    else
                    {
                        object obj = Activator.CreateInstance(newType);
                        // Debug.Log($"Create {newType}: {obj}");
                        if (!valueIsNull)
                        {
                            obj = ReferencePickerAttributeDrawer.CopyObj(value, obj);
                        }
                        if (payload.State != UIToolkitValueEditPayloadState.GenericType)
                        {
                            payload.State = UIToolkitValueEditPayloadState.GenericType;
                            fieldsBodyNew.Clear();
                        }

                        // Debug.Log($"swap {preType} -> {newType}: {obj}; setter={setterOrNull}");

                        beforeSet?.Invoke(value);
                        setterOrNull?.Invoke(obj);
                    }
                }));
                genFoldout.Add(fieldsBodyNew);
                // genFoldout.Add(new VisualElement
                // {
                //     name = "saintsfield-edit-fields",
                // });
            }

            VisualElement fieldsBody = genFoldout.Q<VisualElement>(name: "saintsfield-edit-fields");

            UIToolkitValueEditPayload payload = (UIToolkitValueEditPayload)genFoldout.userData;

            Type valueActualType = payload.UnityObjectOverrideType ?? value?.GetType();
            // Debug.Log($"valueActualType={valueActualType}");
            if (valueActualType != null && typeof(UnityEngine.Object).IsAssignableFrom(valueActualType))
            {
                ObjectField objFieldResult = UIToolkitObjectFieldEdit(fieldsBody.Q<ObjectField>(name: objFieldName), label, valueActualType, (UnityEngine.Object)value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout);
                // Debug.Log($"objFieldResult={objFieldResult}");
                if (objFieldResult != null)
                {
                    objFieldResult.name = objFieldName;
                    fieldsBody.Clear();
                    fieldsBody.Add(objFieldResult);
                    payload.State = UIToolkitValueEditPayloadState.FieldObject;
                    objFieldResult.label = "";
                }

                return useOld? null: genFoldout;
            }

            if (valueIsNull)
            {
                fieldsBody.Clear();
                payload.State = UIToolkitValueEditPayloadState.FieldObject;
                return useOld? null: genFoldout;
            }

            payload.State = UIToolkitValueEditPayloadState.GenericType;

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

            // Debug.Log("Init generic type");
            foreach (FieldInfo fieldInfo in fieldTargets)
            {
                string name = fieldInfo.Name;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
                Debug.Log($"render general field {name}");
#endif
                object fieldValue = fieldInfo.GetValue(value);
                VisualElement result = UIToolkitValueEdit(
                    oldElement?.Q<VisualElement>(name: name),
                    ObjectNames.NicifyVariableName(name),
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
                    inHorizontalLayout);
                // Debug.Log($"{name}: {result}: {fieldInfo.FieldType}");
                // ReSharper disable once InvertIf
                if(result != null)
                {
                    result.name = name;
                    fieldsBody.Add(result);
                }
            }

            foreach (PropertyInfo propertyInfo in propertyTargets)
            {
                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                string name = propertyInfo.Name;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
                Debug.Log($"render general property {name}");
#endif
                object propertyValue = propertyInfo.GetValue(value);

                VisualElement result = UIToolkitValueEdit(
                    oldElement?.Q<VisualElement>(name: name),
                    ObjectNames.NicifyVariableName(name),
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
                    inHorizontalLayout);
                // ReSharper disable once InvertIf
                if(result != null)
                {
                    result.name = name;
                    fieldsBody.Add(result);
                }
            }

            bool enabled = setterOrNull != null;
            if (genFoldout.enabledSelf != enabled)
            {
                genFoldout.SetEnabled(enabled);
                genFoldout.AddToClassList(ClassSaintsFieldEditingDisabled);
            }

            return useOld? null: genFoldout;
        }

        private static ObjectField UIToolkitObjectFieldEdit(VisualElement oldElement, string label, Type valueType, UnityEngine.Object value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            if (oldElement is ObjectField oldUnityEngineObjectField)
            {
                oldUnityEngineObjectField.objectType = valueType;
                oldUnityEngineObjectField.SetValueWithoutNotify(value);
                return null;
            }

            ObjectField element = new ObjectField(label)
            {
                value = value,
                objectType = valueType,
            };
            if (labelGrayColor)
            {
                element.labelElement.style.color = EColor.EditorSeparator.GetColor();
            }
            if (inHorizontalLayout)
            {
                element.style.flexDirection = FlexDirection.Column;
                // element.style.flexWrap = Wrap.Wrap;
                // Label elementLabel = element.Q<Label>();
                // if (elementLabel != null)
                // {
                //     elementLabel.style.minWidth = 0;
                //     elementLabel.style.borderRightWidth = 1;
                //     elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                // }
            }
            else
            {
                element.AddToClassList(ObjectField.alignedFieldUssClassName);
            }
            if (setterOrNull == null)
            {
                element.SetEnabled(false);
                element.AddToClassList(ClassSaintsFieldEditingDisabled);
            }
            else
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }

            return element;
        }

        private static string GetDropdownTypeLabel(Type type)
        {
            return type == null
                ? "null"
                : $"{type.Name}: <color=#{ColorUtility.ToHtmlStringRGB(EColor.Gray.GetColor())}>{type.Namespace}</color>";
        }

        private static UIToolkitUtils.DropdownButtonField MakeTypeDropdown(string label, Type fieldType, object currentValue, Action<Type> setType)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(label);
            dropdownButton.ButtonElement.text = GetDropdownTypeLabel(currentValue?.GetType());

            Type[] optionTypes = ReferencePickerAttributeDrawer
                .GetTypesDerivedFrom(fieldType)
                .ToArray();
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

            foreach (Type type in optionTypes)
            {
                string displayName = GetDropdownTypeLabel(type);
                dropdownList.Add(new AdvancedDropdownList<Type>(displayName, type));
            }

            int optionCount = (canBeNull ? 1 : 0) + optionTypes.Length;

            if (optionCount <= 1)  // no more options, disallow picking
            {
                dropdownButton.style.display = DisplayStyle.None;
            }

            dropdownButton.ButtonElement.clicked += () =>
            {
                AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
                {
                    Error = "",
                    CurDisplay = "null",
                    CurValues = new Type[]{},
                    DropdownListValue = dropdownList,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                };

                Rect worldBound = dropdownButton.worldBound;
                float maxHeight = Screen.height - dropdownButton.worldBound.y - dropdownButton.worldBound.height - 100;
                if (maxHeight < 100)
                {
                    // Debug.LogError($"near out of screen: {maxHeight}");
                    worldBound.y -= 300 + worldBound.height;
                    maxHeight = 300;
                }
                worldBound.height = SaintsPropertyDrawer.SingleLineHeight;
                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    metaInfo,
                    dropdownButton.worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        Type newType = (Type)curItem;
                        if (newType == null)
                        {
                            dropdownButton.ButtonElement.text = "null";
                            setType(null);
                            return;
                        }

                        // if (newType.IsSubclassOf(typeof(ScriptableObject)))
                        // {
                        //     Debug.Log($"is so!");
                        //     // var so = ScriptableObject.CreateInstance(newType);
                        //     setterOrNull((UnityEngine.Object)null);
                        //     return;
                        // }

                        setType(newType);
                        dropdownButton.ButtonElement.text = GetDropdownTypeLabel(newType);
                    }
                ));

            };
            return dropdownButton;
        }

        // private int _listCurPageIndex = 0;
        // private List<int> _listItemIndexToOriginIndex;

        private class ListViewPayload
        {
            public List<object> RawValues;
            public List<int> ItemIndexToOriginIndex;
            public object RawListValue;
        }

        private static Foldout MakeListView(Foldout oldElement, string label, Type valueType, object rawListValue, object[] listValue, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            Foldout foldout = oldElement;
            if (foldout != null && !foldout.ClassListContains("saintsfield-list"))
            {
                foldout = null;
            }
            if (foldout == null)
            {
                // Debug.Log($"Create new Foldout");
                foldout = new Foldout
                {
                    text = label,
                };
                if (labelGrayColor)
                {
                    foldout.style.color = EColor.EditorSeparator.GetColor();
                }
                foldout.AddToClassList("saintsfield-list");
                VisualElement foldoutContent = foldout.Q<VisualElement>(className: "unity-foldout__content");
                if (foldoutContent != null)
                {
                    foldoutContent.style.marginLeft = 0;
                }

                if(setterOrNull != null)
                {
                    // nullable
                    foldout.Q<Toggle>().Add(new Button(() =>
                    {
                        beforeSet(rawListValue);
                        setterOrNull(null);
                    })
                    {
                        // text = "x",
                        tooltip = "Set to null",
                        style =
                        {
                            position = Position.Absolute,
                            // top = -EditorGUIUtility.singleLineHeight,
                            top = 0,
                            right = 0,
                            width = EditorGUIUtility.singleLineHeight,
                            height = EditorGUIUtility.singleLineHeight,

                            backgroundImage = Util.LoadResource<Texture2D>("close.png"),
#if UNITY_2022_2_OR_NEWER
                            backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                            backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                            backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                            backgroundSize = new BackgroundSize(BackgroundSizeType.Contain),
#else
                            unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                        },
                    });
                }
            }

            ListView listView = foldout.Q<ListView>();
            if (listView == null)
            {
                ListViewPayload payload = new ListViewPayload
                {
                    RawValues = listValue.ToList(),
                    ItemIndexToOriginIndex = listValue.Select((_, index) => index).ToList(),
                    RawListValue = rawListValue,
                };
                // Debug.Log($"Create new listView");
                bool showAddRemoveFooter = true;
                if(valueType == typeof(Array) || valueType.IsSubclassOf(typeof(Array)))
                {
                    // Debug.Log("array");
                }
                else if (rawListValue is IList)
                {
                    // Debug.Log("IList");
                }
                else
                {
                    showAddRemoveFooter = false;
                }
                listView = new ListView
                {
                    selectionType = SelectionType.Multiple,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    // showBoundCollectionSize = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0,
                    showBoundCollectionSize = false,
                    showFoldoutHeader = false,
                    headerTitle = label,
                    showAddRemoveFooter = showAddRemoveFooter,
                    reorderMode = ListViewReorderMode.Animated,
                    reorderable = showAddRemoveFooter,
                    style =
                    {
                        flexGrow = 1,
                        position = Position.Relative,
                    },
                    itemsSource = listValue.Select((_, index) => index).ToList(),
                    makeItem = () => new VisualElement(),

                    userData = payload,
                };

                Type elementType = null;
                foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypesFromType(valueType))
                {
                    Type tryGetElementType = ReflectUtils.GetElementType(eachType);
                    // Debug.Log($"{eachType}({eachType.IsGenericType}) -> {tryGetElementType}");
                    if (tryGetElementType != eachType)
                    {
                        elementType = tryGetElementType;
                        break;
                    }
                }

                if (elementType == null)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError($"Failed to find element type in {valueType}");
#endif
                    elementType = typeof(object);
                }

                //
                // Type elementType = ReflectUtils.GetElementType(valueType);
                // Debug.Log(elementType.IsGenericType);
                // if(elementType.IsGenericType)
                // {
                //     Debug.Log(elementType.GetGenericArguments()[0]);
                // }
                // Debug.Log($"elementType={elementType}");

                void BindItem(VisualElement visualElement, int index)
                {
                    // int actualIndex = (int)listView.itemsSource[index];
                    // Debug.Log($"{index} -> {actualIndex}");
                    // Debug.Log($"index={index}, ItemIndexToOriginIndex={string.Join(",", payload.ItemIndexToOriginIndex)}");

                    VisualElement firstChild = visualElement.Children().FirstOrDefault();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                    Debug.Log($"bind {index} with old child: {firstChild}");
#endif

                    int actualIndex = payload.ItemIndexToOriginIndex[index];
                    object actualValue = payload.RawValues[actualIndex];
                    // Debug.Log($"elementType={elementType}, actualValue={actualValue}, rawValues={string.Join(",", payload.RawValues)}");
                    VisualElement item = UIToolkitValueEdit(
                        firstChild,
                        $"Element {actualIndex}",
                        elementType,
                        actualValue,
                        null,
                        showAddRemoveFooter
                         ? newItemValue =>
                            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                                Debug.Log($"List {actualIndex} set newValue {newItemValue}");
#endif
                                IList rawListValueArray = (IList) payload.RawListValue;
                                rawListValueArray[actualIndex] = newItemValue;
                                payload.RawValues[actualIndex] = newItemValue;
                            }
                         : null,
                        false,
                        inHorizontalLayout);
                    if (item != null)
                    {
                        visualElement.Clear();
                        visualElement.Add(item);
                    }
                }

                listView.bindItem = BindItem;

                Button listViewAddButton = listView.Q<Button>("unity-list-view__add-button");
                if(listViewAddButton != null)
                {
                    listViewAddButton.clickable = new Clickable(() =>
                    {
                        int oldSize = payload.RawValues.Count;
                        int newSize = oldSize + 1;
                        object addItem = elementType.IsValueType
                            ? Activator.CreateInstance(elementType)
                            : null;

                        if (valueType == typeof(Array) || valueType.IsSubclassOf(typeof(Array)))
                        {
                            beforeSet.Invoke(rawListValue);
                            Array newArray = Array.CreateInstance(elementType, newSize);
                            payload.RawValues.Add(addItem);
                            Array.Copy(payload.RawValues.ToArray(), newArray, oldSize);
                            payload.RawListValue = newArray;
                            setterOrNull?.Invoke(newArray);
                        }
                        else
                        {
                            IList rawListValueArray = (IList)payload.RawListValue;
                            rawListValueArray.Add(addItem);
                            payload.RawValues.Add(addItem);
                            payload.ItemIndexToOriginIndex = payload.RawValues.Select((_, index) => index).ToList();
                            listView.itemsSource = payload.ItemIndexToOriginIndex.ToList();
                        }
                    });
                }

                listView.itemsRemoved += objects =>
                {
                    List<int> removeIndexInRaw = objects
                        .Select(removeIndex => payload.ItemIndexToOriginIndex[removeIndex])
                        .OrderByDescending(each => each)
                        .ToList();

                    if(valueType == typeof(Array) || valueType.IsSubclassOf(typeof(Array)))
                    {
                        beforeSet.Invoke(rawListValue);
                        Array newArray = Array.CreateInstance(elementType, payload.RawValues.Count - removeIndexInRaw.Count());
                        Array rawArray = (Array) payload.RawListValue;
                        int copyIndex = 0;
                        foreach ((object rawValue, int rawIndex) in rawArray.Cast<object>().WithIndex())
                        {
                            if (removeIndexInRaw.Contains(rawIndex))
                            {
                                continue;
                            }

                            newArray.SetValue(rawValue, copyIndex);
                            copyIndex++;
                        }
                        // payload.RawValues.Add(addItem);
                        // Array.Copy(payload.RawValues.ToArray(), newArray, oldSize);
                        payload.RawListValue = newArray;
                        setterOrNull?.Invoke(newArray);
                    }
                    else
                    {
                        IList rawListValueArray = (IList) payload.RawListValue;
                        foreach (int removeIndex in removeIndexInRaw)
                        {
                            rawListValueArray.RemoveAt(removeIndex);
                        }
                    }
                };

                listView.itemIndexChanged += (first, second) =>
                {
                    int fromPropIndex = payload.ItemIndexToOriginIndex[first];
                    int toPropIndex = payload.ItemIndexToOriginIndex[second];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
                    Debug.Log($"drag {fromPropIndex}({first}) -> {toPropIndex}({second}); ItemIndexToOriginIndex={string.Join(",", payload.ItemIndexToOriginIndex)}");
#endif

                    IList lis = (IList)payload.RawListValue;
                    MoveArrayElement(lis, fromPropIndex, toPropIndex);
                    // (lis[fromPropIndex], lis[toPropIndex]) = (lis[toPropIndex], lis[fromPropIndex]);
                    // payload.RawValues = lis.Cast<object>().ToList();
                    // (payload.RawValues[fromPropIndex], payload.RawValues[toPropIndex]) = (payload.RawValues[toPropIndex], payload.RawValues[fromPropIndex]);
                    // (payload.ItemIndexToOriginIndex[fromPropIndex], payload.ItemIndexToOriginIndex[toPropIndex]) = (payload.ItemIndexToOriginIndex[toPropIndex], payload.ItemIndexToOriginIndex[fromPropIndex]);
                    // payload.ItemIndexToOriginIndex = payload.RawValues.Select((_, index) => index).ToList();
                    // listView.Rebuild();
                };

                foldout.Add(listView);
            }

            ListViewPayload oldPayload = (ListViewPayload)listView.userData;
            oldPayload.RawValues = listValue.ToList();
            oldPayload.RawListValue = rawListValue;

            // Debug.Log($"Refresh count={listValue.Length}");
            oldPayload.ItemIndexToOriginIndex = oldPayload.RawValues.Select((_, index) => index).ToList();
            listView.itemsSource = oldPayload.ItemIndexToOriginIndex.ToList();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_NATIVE_PROPERTY_RENDERER
            Debug.Log($"ItemIndexToOriginIndex={string.Join(",", oldPayload.ItemIndexToOriginIndex)}");
#endif
            // Debug.Log($"itemSource({listView.itemsSource.Count})={string.Join(",", listView.itemsSource)}");
            // if (listValue.Length > 0)
            // {
            //     Debug.Log($"0 listValue={listValue[0]}; listView.itemsSource={listView.itemsSource[0]}");
            // }
            // listView.Rebuild();

            return oldElement == null? foldout : null;
        }

        private class DictionaryViewPayload
        {
            public object RawDictValue;
            private readonly PropertyInfo _keysProperty;
            private readonly PropertyInfo _indexerProperty;
            private readonly MethodInfo _removeMethod;
            private readonly MethodInfo _containesKeyMethod;

            public DictionaryViewPayload(object rawDictValue, PropertyInfo keysProperty, PropertyInfo indexerProperty,
                MethodInfo removeMethod, MethodInfo containsKeyMethod)
            {
                RawDictValue = rawDictValue;
                _keysProperty = keysProperty;
                _indexerProperty = indexerProperty;
                _removeMethod = removeMethod;
                _containesKeyMethod = containsKeyMethod;
            }

            public IEnumerable<object> GetKeys() => ((IEnumerable)_keysProperty.GetValue(RawDictValue)).Cast<object>();

            public object GetValue(object key) => _indexerProperty.GetValue(RawDictValue, new[] { key });
            public void DeleteKey(object key) => _removeMethod.Invoke(RawDictValue, new[] { key });
            public void SetKeyValue(object key, object value) => _indexerProperty.SetValue(RawDictValue, value, new[] { key });
            public bool ContainsKey(object key) => (bool)_containesKeyMethod.Invoke(RawDictValue, new[] { key });
        }

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK
        private static Foldout MakeDictionaryView(Foldout oldElement, string label, Type valueType, object rawDictValue, bool isReadOnly, Type dictKeyType, Type dictValueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            // Debug.Log(dictKeyType);
            // Debug.Log(dictValueType);

            Foldout foldout = oldElement;
            if (foldout != null && !foldout.ClassListContains("saintsfield-dictionary"))
            {
                foldout = null;
            }

            bool useOld = foldout != null;

            if (foldout == null)
            {
                // Debug.Log($"Create new Foldout");
                foldout = new Foldout
                {
                    text = label,
                };
                if (labelGrayColor)
                {
                    foldout.style.color = EColor.EditorSeparator.GetColor();
                }
                foldout.AddToClassList("saintsfield-dictionary");
                VisualElement foldoutContent = foldout.Q<VisualElement>(className: "unity-foldout__content");
                if (foldoutContent != null)
                {
                    foldoutContent.style.marginLeft = 0;
                }

                if(setterOrNull != null)
                {
                    // nullable
                    foldout.Q<Toggle>().Add(new Button(() =>
                    {
                        beforeSet?.Invoke(rawDictValue);
                        setterOrNull(null);
                    })
                    {
                        // text = "x",
                        tooltip = "Set to null",
                        style =
                        {
                            position = Position.Absolute,
                            // top = -EditorGUIUtility.singleLineHeight,
                            top = 0,
                            right = 0,
                            width = EditorGUIUtility.singleLineHeight,
                            height = EditorGUIUtility.singleLineHeight,

                            backgroundImage = Util.LoadResource<Texture2D>("close.png"),
#if UNITY_2022_2_OR_NEWER
                            backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                            backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                            backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                            backgroundSize = new BackgroundSize(BackgroundSizeType.Contain),
#else
                            unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                        },
                    });
                }
            }

            MultiColumnListView listView = foldout.Q<MultiColumnListView>();
            if (listView == null)
            {
                PropertyInfo keysProperty =  valueType.GetProperty("Keys");
                Debug.Assert(keysProperty != null, $"Failed to get keys from {valueType}");

                PropertyInfo indexerProperty = valueType.GetProperty("Item", new []{dictKeyType});
                Debug.Assert(keysProperty != null, $"Failed to get key indexer from {valueType}");

                MethodInfo removeMethod = valueType.GetMethod("Remove", new[]{dictKeyType});
                Debug.Assert(keysProperty != null, $"Failed to get `Remove` function from {valueType}");

                MethodInfo containsKeyMethod = valueType.GetMethod("ContainsKey", new[]{dictKeyType});

                Debug.Assert(rawDictValue != null, "Dictionary value should not be null");

                DictionaryViewPayload payload = new DictionaryViewPayload(rawDictValue, keysProperty, indexerProperty, removeMethod, containsKeyMethod);

                listView = new MultiColumnListView
                {
                    selectionType = SelectionType.Multiple,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    // showBoundCollectionSize = listDrawerSettingsAttribute.NumberOfItemsPerPage <= 0,
                    showBoundCollectionSize = false,
                    showFoldoutHeader = false,
                    // headerTitle = label,
                    showAddRemoveFooter = !isReadOnly,
                    reorderMode = ListViewReorderMode.Animated,
                    reorderable = false,
                    style =
                    {
                        flexGrow = 1,
                        position = Position.Relative,
                    },
                    itemsSource = payload.GetKeys().ToList(),
                    userData = payload,
                };

                listView.columns.Add(new Column
                {
                    name = "Keys",
                    // title = "Keys",
                    stretchable = true,
                    makeHeader = () =>
                    {
                        VisualElement header = new VisualElement();
                        header.Add(new Label("Keys"));
                        // ToolbarSearchField keySearch = new ToolbarSearchField
                        // {
                        //     // isDelayed = true,
                        //     style =
                        //     {
                        //         marginRight = 3,
                        //         display = saintsDictionaryAttribute?.Searchable ?? true
                        //             ? DisplayStyle.Flex
                        //             : DisplayStyle.None,
                        //         width = Length.Percent(97f),
                        //     },
                        // };
                        // TextField keySearchText = keySearch.Q<TextField>();
                        // if (keySearchText != null)
                        // {
                        //     keySearchText.isDelayed = true;
                        // }
                        // header.Add(keySearch);
                        // keySearch.RegisterValueChangedCallback(evt =>
                        // {
                        //     // Debug.Log($"key search {evt.newValue}");
                        //     if(evt.newValue != preKeySearch)
                        //     {
                        //         RefreshList(evt.newValue, preValueSearch, prePageIndex, numberOfItemsPerPage.value);
                        //     }
                        // });
                        return header;
                    },
                    makeCell = () => new VisualElement(),
                    bindCell = (element, elementIndex) =>
                    {
                        object key = listView.itemsSource[elementIndex];
                        object oldValue = payload.GetValue(key);
                        bool keyChanged = true;

                        VisualElement keyChild = element.Children().FirstOrDefault();

                        element.schedule.Execute(() =>
                        {
                            if (!keyChanged)
                            {
                                return;
                            }

                            keyChanged = false;

                            VisualElement editing = UIToolkitValueEdit(keyChild, "", dictKeyType, key, oldKey =>
                            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                                Debug.Log($"oldKey={oldKey}");
#endif
                                oldValue = payload.GetValue(oldKey);
                                payload.DeleteKey(oldKey);
                            }, newKey =>
                            {
                                if (RuntimeUtil.IsNull(newKey))
                                {
                                    Debug.LogWarning($"Setting key to null is not supported and is ignored");
                                    return;
                                }

                                if (payload.ContainsKey(newKey))
                                {
                                    Debug.LogWarning($"Setting key {key} to existing key {newKey} is not supported and is ignored");
                                    return;
                                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                                Debug.Log($"dictionary editing key {key} -> {newKey}");
#endif
                                // object oldValue = payload.GetValue(key);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                                Debug.Log($"set key {key} -> {newKey} with value {oldValue}");
#endif
                                payload.DeleteKey(key);
                                payload.SetKeyValue(newKey, oldValue);
                                // int sourceIndex = listView.itemsSource.IndexOf(key);
                                // listView.itemsSource[sourceIndex] = newKey;
                                key = newKey;
                                keyChanged = true;
                            }, false, true);

                            if (editing != null)
                            {
                                element.Clear();
                                element.Add(editing);
                            }
                        }).Every(100);
                    },
                });

                listView.columns.Add(new Column
                {
                    name = "Values",
                    // title = "Keys",
                    stretchable = true,
                    makeHeader = () =>
                    {
                        VisualElement header = new VisualElement();
                        header.Add(new Label("Values"));
                        // ToolbarSearchField keySearch = new ToolbarSearchField
                        // {
                        //     // isDelayed = true,
                        //     style =
                        //     {
                        //         marginRight = 3,
                        //         display = saintsDictionaryAttribute?.Searchable ?? true
                        //             ? DisplayStyle.Flex
                        //             : DisplayStyle.None,
                        //         width = Length.Percent(97f),
                        //     },
                        // };
                        // TextField keySearchText = keySearch.Q<TextField>();
                        // if (keySearchText != null)
                        // {
                        //     keySearchText.isDelayed = true;
                        // }
                        // header.Add(keySearch);
                        // keySearch.RegisterValueChangedCallback(evt =>
                        // {
                        //     // Debug.Log($"key search {evt.newValue}");
                        //     if(evt.newValue != preKeySearch)
                        //     {
                        //         RefreshList(evt.newValue, preValueSearch, prePageIndex, numberOfItemsPerPage.value);
                        //     }
                        // });
                        return header;
                    },
                    makeCell = () => new VisualElement(),
                    bindCell = (element, elementIndex) =>
                    {
                        object key = listView.itemsSource[elementIndex];
                        object value = payload.GetValue(key);

                        VisualElement valueChild = element.Children().FirstOrDefault();

                        VisualElement editing = UIToolkitValueEdit(valueChild, "", dictValueType, value, null, newValue =>
                        {
                            object refreshedKey = listView.itemsSource[elementIndex];
                            payload.SetKeyValue(refreshedKey, newValue);
                        }, false, true);

                        if (editing != null)
                        {
                            element.Clear();
                            element.Add(editing);
                        }
                    },
                });

                listView.itemsRemoved += ints =>
                {
                    int[] toRemoveIndices = ints.ToArray();
                    // List<object> keepKeys = new List<object>();
                    List<object> removeKeys = new List<object>();
                    int index = 0;
                    foreach (object key in listView.itemsSource)
                    {
                        if (Array.IndexOf(toRemoveIndices, index) != -1)
                        {
                            removeKeys.Add(key);
                        }
                        index++;
                    }

                    foreach (object key in removeKeys)
                    {
                        payload.DeleteKey(key);
                        // listView.itemsSource.Remove(key);
                    }

                    // listView.itemsSource = keepKeys;
                };

                Button listViewAddButton = listView.Q<Button>("unity-list-view__add-button");

                const int pairPanelBorderWidth = 1;
                Color pairPanelBorderColor = EColor.EditorEmphasized.GetColor();
                VisualElement addPairPanel = new VisualElement
                {
                    style =
                    {
                        display = DisplayStyle.None,

                        borderLeftWidth = pairPanelBorderWidth,
                        borderRightWidth = pairPanelBorderWidth,
                        borderTopWidth = pairPanelBorderWidth,
                        borderBottomWidth = pairPanelBorderWidth,

                        borderTopColor = pairPanelBorderColor,
                        borderBottomColor = pairPanelBorderColor,
                        borderLeftColor = pairPanelBorderColor,
                        borderRightColor = pairPanelBorderColor,

                        marginTop = 1,
                        marginBottom = 1,
                        marginLeft = 1,
                        marginRight = 1,
                    },
                };

                VisualElement addPairActionContainer = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                    },
                };

                Button addPairConfirmButton = new Button
                {
                    text = "OK",
                    style =
                    {
                        flexGrow = 1,
                    },
                };
                addPairActionContainer.Add(addPairConfirmButton);
                Button addPairCancleButton = new Button(() =>
                {
                    listViewAddButton.SetEnabled(true);
                    addPairPanel.style.display = DisplayStyle.None;
                })
                {
                    text = "Cancel",
                    style =
                    {
                        flexGrow = 1,
                    },
                };
                addPairActionContainer.Add(addPairCancleButton);

                VisualElement addPairKeyContainer = new VisualElement();
                addPairPanel.Add(addPairKeyContainer);
                object addPairKey = dictKeyType.IsValueType ? Activator.CreateInstance(dictKeyType) : null;
                bool addPairKeyChange = true;
                addPairKeyContainer.schedule.Execute(() =>
                {
                    if (!addPairKeyChange)
                    {
                        return;
                    }

                    VisualElement r = UIToolkitValueEdit(
                        addPairKeyContainer.Children().FirstOrDefault(),
                        "Key",
                        dictKeyType,
                        addPairKey,
                        null,
                        newKey =>
                        {
                            bool invalidKey = RuntimeUtil.IsNull(newKey);
                            if (!invalidKey)
                            {
                                invalidKey = payload.ContainsKey(newKey);
                            }

                            addPairConfirmButton.SetEnabled(!invalidKey);
                            if (!invalidKey)
                            {
                                addPairKey = newKey;
                                addPairKeyChange = true;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                                Debug.Log($"set new pair key {newKey}");
#endif
                            }
                        },
                        false,
                        inHorizontalLayout
                    );
                    // ReSharper disable once InvertIf
                    if (r != null)
                    {
                        addPairKeyContainer.Clear();
                        addPairKeyContainer.Add(r);
                    }

                    addPairKeyChange = false;
                }).Every(100);

                VisualElement addPairValueContainer = new VisualElement();
                addPairPanel.Add(addPairValueContainer);
                object addPairValue = dictValueType.IsValueType ? Activator.CreateInstance(dictValueType) : null;
                bool addPairValueChanged = true;
                addPairValueContainer.schedule.Execute(() =>
                {
                    if (!addPairValueChanged)
                    {
                        return;
                    }

                    VisualElement r = UIToolkitValueEdit(
                        addPairValueContainer.Children().FirstOrDefault(),
                        "Value",
                        dictValueType,
                        addPairValue,
                        null,
                        newValue =>
                        {
                            addPairValue = newValue;
                            addPairValueChanged = true;
                        },
                        false,
                        inHorizontalLayout
                    );
                    // ReSharper disable once InvertIf
                    if (r != null)
                    {
                        addPairValueContainer.Clear();
                        addPairValueContainer.Add(r);
                    }

                    addPairValueChanged = false;
                }).Every(100);

                addPairConfirmButton.clicked += () =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_DICTIONARY
                    Debug.Log($"dictionary set {addPairKey} -> {addPairValue}");
#endif
                    payload.SetKeyValue(addPairKey, addPairValue);
                    addPairPanel.style.display = DisplayStyle.None;
                    listViewAddButton.SetEnabled(true);
                    listView.itemsSource = payload.GetKeys().ToList();
                    // setterOrNull(payload.RawDictValue);
                    // listView.Rebuild();
                };

                addPairPanel.Add(addPairActionContainer);

                if (!isReadOnly)
                {
                    listViewAddButton.clickable = new Clickable(() =>
                    {
                        listViewAddButton.SetEnabled(false);
                        addPairConfirmButton.SetEnabled(!RuntimeUtil.IsNull(addPairKey) && !payload.ContainsKey(addPairKey));
                        addPairPanel.style.display = DisplayStyle.Flex;
                    });
                }

                foldout.Add(listView);
                foldout.Add(addPairPanel);
            }

            DictionaryViewPayload oldPayload = (DictionaryViewPayload)listView.userData;
            oldPayload.RawDictValue = rawDictValue;
            if (rawDictValue != null)
            {
                // Debug.Log($"Refresh listView");
                listView.itemsSource = oldPayload.GetKeys().ToList();
            }

            return useOld? null : foldout;
        }
#endif
        private static void MoveArrayElement(IList list, int fromIndex, int toIndex)
        {
            if (list == null)
            {
#if SAINTSFIELD_DEBUG
                throw new ArgumentNullException(nameof(list));
#endif
#pragma warning disable CS0162 // Unreachable code detected
                // ReSharper disable once HeuristicUnreachableCode
                return;
#pragma warning restore CS0162 // Unreachable code detected
            }
            if (fromIndex < 0 || fromIndex >= list.Count)
            {
#if SAINTSFIELD_DEBUG
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
#endif
#pragma warning disable CS0162 // Unreachable code detected
                // ReSharper disable once HeuristicUnreachableCode
                return;
#pragma warning restore CS0162 // Unreachable code detected
            }
            if (toIndex < 0 || toIndex >= list.Count)
            {
#if SAINTSFIELD_DEBUG
                throw new ArgumentOutOfRangeException(nameof(toIndex));
#endif
#pragma warning disable CS0162 // Unreachable code detected
                // ReSharper disable once HeuristicUnreachableCode
                return;
#pragma warning restore CS0162 // Unreachable code detected
            }

            if (fromIndex == toIndex)
            {
                return;
            }

            // shifting
            object item = list[fromIndex];

            if (fromIndex < toIndex)
            {
                for (int i = fromIndex; i < toIndex; i++)
                {
                    list[i] = list[i + 1];
                }
            }
            else
            {
                for (int i = fromIndex; i > toIndex; i--)
                {
                    list[i] = list[i - 1];
                }
            }

            list[toIndex] = item;
        }
    }
}
#endif
