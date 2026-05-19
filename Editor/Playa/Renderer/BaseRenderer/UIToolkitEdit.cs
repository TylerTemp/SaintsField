using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AnimatorParamDrawer;
using SaintsField.Editor.Drawers.AnimatorStateDrawer;
using SaintsField.Editor.Drawers.CurveRangeDrawer;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer;
using SaintsField.Editor.Drawers.GuidDrawer;
using SaintsField.Editor.Drawers.InputAxisDrawer;
using SaintsField.Editor.Drawers.LayerDrawer;
using SaintsField.Editor.Drawers.MinMaxSliderDrawer;
using SaintsField.Editor.Drawers.ProgressBarDrawer;
using SaintsField.Editor.Drawers.PropRangeDrawer;
using SaintsField.Editor.Drawers.RateDrawer;
using SaintsField.Editor.Drawers.ResizableTextAreaDrawer;
using SaintsField.Editor.Drawers.SaintsDecimalType;
using SaintsField.Editor.Drawers.SaintsDictionary;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer;
using SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer;
using SaintsField.Editor.Drawers.SortingLayerDrawer;
using SaintsField.Editor.Drawers.TagDrawer;
using SaintsField.Editor.Drawers.TimeSpanDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.Playa.Renderer.ListDrawerSettings;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public static class UIToolkitEdit
    {
        public static (VisualElement result, bool isNestedField) UIToolkitValueEdit(VisualElement oldElement,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RENDERER_VALUE_EDIT
            Debug.Log($"render start {label}/{valueType}/{value}");
#endif
            if (valueType == typeof(Placeholder))
            {
                return (null, false);
            }

            // Color reColor = EColor.EditorSeparator.GetColor();

            foreach (Attribute attribute in allAttributes)
            {
                switch (attribute)
                {
                    case ValueButtonsAttribute valueButtonsAttribute:
                    {
                        return (ValueButtonsAttributeDrawer.UIToolkitValueEdit(
                            oldElement,
                            valueButtonsAttribute,
                            label,
                            value,
                            valueType,
                            beforeSet,
                            setterOrNull,
                            labelGrayColor,
                            inHorizontalLayout,
                            allAttributes,
                            targets), false);
                    }
                    case DropdownAttribute treeDropdownAttribute:
                    {
                        return (TreeDropdownAttributeDrawer.UIToolkitValueEdit(
                            oldElement,
                            treeDropdownAttribute,
                            label,
                            valueType,
                            value,
                            beforeSet,
                            setterOrNull,
                            labelGrayColor,
                            inHorizontalLayout,
                            allAttributes,
                            targets, richTextTagProvider), false);
                    }
                }
            }

            #region bool
            if (valueType == typeof(bool) || value is bool)
            {
                if (oldElement is Toggle oldToggle)
                {
                    oldToggle.SetValueWithoutNotify(Convert.ToBoolean(value));
                    return (null, false);
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

                    VisualElement unityToggleInput = element.Q<VisualElement>(className: "unity-toggle__input");
                    if(unityToggleInput != null)
                    {
                        element.style.flexDirection = FlexDirection.RowReverse;
                        element.style.justifyContent = Justify.FlexEnd;
                        unityToggleInput.style.flexGrow = 0;
                        // unityToggleInput.style.flexGrow = 0;
                    }
                }
                else
                {
                    element.AddToClassList(Toggle.alignedFieldUssClassName);
                }
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region sbyte
            if (valueType == typeof(sbyte) || value is sbyte)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditSByte(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (sbyte) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case ProgressBarAttribute progressBarAttribute:
                            return (ProgressBarAttributeDrawer.UIToolkitValueEditSByte(
                                oldElement,
                                progressBarAttribute,
                                label,
                                (sbyte) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }
                if (oldElement is IntegerField integerField)
                {
                    integerField.SetValueWithoutNotify((sbyte)value);
                    return (null, false);
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (sbyte)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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

                return (element, false);
            }
            #endregion

            #region byte
            if (valueType == typeof(byte) || value is byte)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditByte(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (byte) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case ProgressBarAttribute progressBarAttribute:
                            return (ProgressBarAttributeDrawer.UIToolkitValueEditByte(
                                oldElement,
                                progressBarAttribute,
                                label,
                                (byte) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }

                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((byte)value);
                    return (null, false);
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (byte)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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

                return (element, false);
            }
            #endregion

            #region short
            if (valueType == typeof(short) || value is short)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditShort(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (short) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case ProgressBarAttribute progressBarAttribute:
                            return (ProgressBarAttributeDrawer.UIToolkitValueEditShort(
                                oldElement,
                                progressBarAttribute,
                                label,
                                (short) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }

                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((short)value);
                    return (null, false);
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (short)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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
                return (element, false);
            }
            #endregion

            #region ushort
            if (valueType == typeof(ushort) || value is ushort)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditUShort(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (ushort) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case ProgressBarAttribute progressBarAttribute:
                            return (ProgressBarAttributeDrawer.UIToolkitValueEditUShort(
                                oldElement,
                                progressBarAttribute,
                                label,
                                (ushort) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }

                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((ushort)value);
                    return (null, false);
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (ushort)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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

                return (element, false);
            }
            #endregion

            #region int
            if (valueType == typeof(int) || value is int)
            {
                foreach (Attribute each in allAttributes)
                {
                    switch (each)
                    {
                        case LayerAttribute:
                            return (LayerAttributeDrawer.UIToolkitValueEditInt(oldElement, label, (int)value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout, allAttributes), false);
                        case SceneAttribute sceneAttribute:
                            return (SceneAttributeDrawer.UIToolkitValueEditInt(
                                oldElement,
                                sceneAttribute,
                                label,
                                (int) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
                        case SortingLayerAttribute sortingLayerAttribute:
                            return (SortingLayerAttributeDrawer.UIToolkitValueEditInt(
                                oldElement,
                                sortingLayerAttribute,
                                label,
                                (int) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
#if UNITY_2021_2_OR_NEWER
                        case ShaderParamAttribute shaderParamAttribute:
                            return (ShaderParamAttributeDrawer.UIToolkitValueEditInt(
                                oldElement,
                                shaderParamAttribute,
                                label,
                                (int) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
#endif
                        case RateAttribute rateAttribute:
                            return (RateAttributeDrawer.UIToolkitValueEdit(
                                oldElement,
                                rateAttribute,
                                label,
                                (int) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditInt(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (int) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case ProgressBarAttribute progressBarAttribute:
                            return (ProgressBarAttributeDrawer.UIToolkitValueEditInt(
                                oldElement,
                                progressBarAttribute,
                                label,
                                (int) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case AnimatorParamAttribute animatorParamAttribute:
                            return (AnimatorParamAttributeDrawer.UIToolkitValueEditInt(
                                oldElement,
                                animatorParamAttribute,
                                label,
                                (int) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }

                if (oldElement is IntegerField oldIntegerField)
                {
                    oldIntegerField.SetValueWithoutNotify((int)value);
                    return (null, false);
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (int)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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

                return (element, false);
            }
            #endregion

            #region unit
            if (valueType == typeof(uint) || value is uint)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditUInt(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (uint) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }

#if UNITY_2022_3_OR_NEWER
                if (oldElement is UnsignedIntegerField oldUnsignedIntegerField)
                {
                    oldUnsignedIntegerField.SetValueWithoutNotify((uint)value);
                    return (null, false);
                }

                UnsignedIntegerField element = new UnsignedIntegerField(label)
                {
                    value = (uint)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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
                    return (null, false);
                }

                IntegerField element = new IntegerField(label)
                {
                    value = (int)(uint)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = ReColor;
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

                return (element, false);
            }
            #endregion

            #region long
            if (valueType == typeof(long) || value is long)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case DateTimeAttribute _:
                            return (DateTimeAttributeDrawer.UIToolkitValueEdit(
                                oldElement,
                                label,
                                valueType,
                                value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
                        case TimeSpanAttribute _:
                            return (TimeSpanAttributeDrawer.UIToolkitValueEdit(
                                oldElement,
                                label,
                                valueType,
                                value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditLong(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (long) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }


                if (oldElement is LongField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify((long)value);
                    return (null, false);
                }

                LongField element = new LongField(label)
                {
                    value = (long)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region ulong
            if (valueType == typeof(ulong) || value is ulong)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditULong(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (ulong) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }
#if UNITY_2022_3_OR_NEWER
                ulong ulongRawValue = (ulong)value;
                if (oldElement is UnsignedLongField oldLongField)
                {
                    oldLongField.SetValueWithoutNotify(ulongRawValue);
                    return (null, false);
                }

                UnsignedLongField element = new UnsignedLongField(label)
                {
                    value = ulongRawValue,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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
                    return (null, false);
                }

                LongField element = new LongField(label)
                {
                    value = ulongRawValue,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = ReColor;
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

                return (element, false);
            }
            #endregion

            #region float
            if (valueType == typeof(float) || value is float)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditFloat(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (float) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case ProgressBarAttribute progressBarAttribute:
                            return (ProgressBarAttributeDrawer.UIToolkitValueEditFloat(
                                oldElement,
                                progressBarAttribute,
                                label,
                                (float) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }
                if (oldElement is FloatField oldFloatField)
                {
                    oldFloatField.SetValueWithoutNotify((float)value);
                    return (null, false);
                }

                FloatField element = new FloatField(label)
                {
                    value = (float)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region double
            if (valueType == typeof(double) || value is double)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case PropRangeAttribute propRangeAttribute:
                            return (PropRangeAttributeDrawer.UIToolkitValueEditDouble(
                                oldElement,
                                propRangeAttribute,
                                label,
                                (double) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case ProgressBarAttribute progressBarAttribute:
                            return (ProgressBarAttributeDrawer.UIToolkitValueEditDouble(
                                oldElement,
                                progressBarAttribute,
                                label,
                                (double) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }

                if (oldElement is DoubleField oldDoubleField)
                {
                    oldDoubleField.SetValueWithoutNotify((double)value);
                    return (null, false);
                }

                DoubleField element = new DoubleField(label)
                {
                    value = (double)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region string
            if (valueType == typeof(string) || value is string)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case LayerAttribute:
                            return (LayerAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
                        case SceneAttribute sceneAttribute:
                            return (SceneAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                sceneAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
                        case GuidAttribute guidAttribute:
                            return (GuidAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                guidAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
                        case SortingLayerAttribute sortingLayerAttribute:
                            return (SortingLayerAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                sortingLayerAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
                        case TagAttribute tagAttribute:
                            return (TagAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                tagAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
                        case InputAxisAttribute inputAxisAttribute:
                            return (InputAxisAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                inputAxisAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes), false);
#if UNITY_2021_2_OR_NEWER
                        case ShaderParamAttribute shaderParamAttribute:
                            return (ShaderParamAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                shaderParamAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case ShaderKeywordAttribute shaderKeywordAttribute:
                            return (ShaderKeywordAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                shaderKeywordAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
#endif

                        case AnimatorParamAttribute animatorParamAttribute:
                            return (AnimatorParamAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                animatorParamAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case AnimatorStateAttribute animatorStateAttribute:
                            return (AnimatorStateAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                animatorStateAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                        case ResizableTextAreaAttribute resizableTextAreaAttribute:
                            return (ResizableTextAreaAttributeDrawer.UIToolkitValueEditString(
                                oldElement,
                                resizableTextAreaAttribute,
                                label,
                                (string) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }

                if (oldElement is TextField oldTextField)
                {
                    oldTextField.SetValueWithoutNotify((string)value);
                    return (null, false);
                }

                TextField element = new TextField(label)
                {
                    value = (string)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region char
            if (valueType == typeof(char) || value is char)
            {
                if (oldElement is TextField oldTextField)
                {
                    oldTextField.maxLength = 1;
                    oldTextField.SetValueWithoutNotify($"{value}");
                    return (null, false);
                }

                TextField element = new TextField(label)
                {
                    value = $"{value}",
                    maxLength = 1,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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

                return (element, false);
            }
            #endregion

            #region Vector2
            if (valueType == typeof(Vector2) || value is Vector2)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case MinMaxSliderAttribute minMaxSliderAttribute:
                            return (MinMaxSliderAttributeDrawer.UIToolkitValueEditVector2(
                                oldElement,
                                minMaxSliderAttribute,
                                label,
                                (Vector2) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }

                if (oldElement is Vector2Field oldVector2Field)
                {
                    oldVector2Field.SetValueWithoutNotify((Vector2)value);
                    return (null, false);
                }

                Vector2Field element = new Vector2Field(label)
                {
                    value = (Vector2)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region Vector3
            if (valueType == typeof(Vector3) || value is Vector3)
            {
                if (oldElement is Vector3Field oldVector3Field)
                {
                    oldVector3Field.SetValueWithoutNotify((Vector3)value);
                    return (null, false);
                }

                Vector3Field element = new Vector3Field(label)
                {
                    value = (Vector3)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region Vector4
            if (valueType == typeof(Vector4) || value is Vector4)
            {
                if (oldElement is Vector4Field oldVector4Field)
                {
                    oldVector4Field.SetValueWithoutNotify((Vector4)value);
                    return (null, false);
                }

                Vector4Field element = new Vector4Field(label)
                {
                    value = (Vector4)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region Vector2Int
            if (valueType == typeof(Vector2Int) || value is Vector2Int)
            {
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case MinMaxSliderAttribute minMaxSliderAttribute:
                            return (MinMaxSliderAttributeDrawer.UIToolkitValueEditVector2Int(
                                oldElement,
                                minMaxSliderAttribute,
                                label,
                                (Vector2Int) value,
                                beforeSet,
                                setterOrNull,
                                labelGrayColor,
                                inHorizontalLayout,
                                allAttributes,
                                targets), false);
                    }
                }

                if (oldElement is Vector2IntField oldVector2IntField)
                {
                    oldVector2IntField.SetValueWithoutNotify((Vector2Int)value);
                    return (null, false);
                }

                Vector2IntField element = new Vector2IntField(label)
                {
                    value = (Vector2Int)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region Vector3Int
            if (valueType == typeof(Vector3Int) || value is Vector3Int)
            {
                if (oldElement is Vector3IntField oldVector3IntField)
                {
                    oldVector3IntField.SetValueWithoutNotify((Vector3Int)value);
                    return (null, false);
                }

                Vector3IntField element = new Vector3IntField(label)
                {
                    value = (Vector3Int)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region Color
            if (valueType == typeof(Color) || value is Color)
            {
                if (oldElement is ColorField oldColorField)
                {
                    oldColorField.SetValueWithoutNotify((Color)value);
                    return (null, false);
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
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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

                return (element, false);
            }
            #endregion

            #region Bounds
            if (valueType == typeof(Bounds) || value is Bounds)
            {
                if (oldElement is BoundsField oldBoundsField)
                {
                    oldBoundsField.SetValueWithoutNotify((Bounds)value);
                    return (null, false);
                }

                BoundsField element = new BoundsField(label)
                {
                    value = (Bounds)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region Rect
            if (valueType == typeof(Rect) || value is Rect)
            {
                if (oldElement is RectField oldRectField)
                {
                    oldRectField.SetValueWithoutNotify((Rect)value);
                    return (null, false);
                }

                RectField element = new RectField(label)
                {
                    value = (Rect)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region RectInt
            if (valueType == typeof(RectInt) || value is RectInt)
            {
                if (oldElement is RectIntField oldRectIntField)
                {
                    oldRectIntField.SetValueWithoutNotify((RectInt)value);
                    return (null, false);
                }

                RectIntField element = new RectIntField(label)
                {
                    value = (RectInt)value,
                };
                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
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
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }

                return (element, false);
            }
            #endregion

            #region Enum
            if (valueType.BaseType == typeof(Enum) || value is Enum)
            {
                Type enumType = valueType.BaseType == typeof(Enum)
                    ? valueType
                    : value.GetType();
                bool isFlag = Attribute.IsDefined(enumType, typeof(FlagsAttribute));

                // Debug.Log(string.Join(",", allAttributes));
                foreach (Attribute attribute in allAttributes)
                {
                    switch (attribute)
                    {
                        case EnumToggleButtonsAttribute enumToggleButtonsAttribute:
                        {
                            if(isFlag)
                            {
                                return (EnumToggleButtonsAttributeDrawer.UIToolkitValueEdit(
                                    oldElement,
                                    enumToggleButtonsAttribute,
                                    label,
                                    value,
                                    enumType,
                                    beforeSet,
                                    setterOrNull,
                                    labelGrayColor,
                                    inHorizontalLayout,
                                    allAttributes,
                                    targets), false);
                            }
                            else
                            {
                                return (ValueButtonsAttributeDrawer.UIToolkitValueEditEnum(
                                    oldElement,
                                    new ValueButtonsAttribute(),
                                    label,
                                    value,
                                    enumType,
                                    beforeSet,
                                    setterOrNull,
                                    labelGrayColor,
                                    inHorizontalLayout,
                                    allAttributes,
                                    targets), false);
                            }
                        }
                    }
                }

                return (TreeDropdownAttributeDrawer.DrawEnumUIToolkit(oldElement, label, valueType, value, beforeSet,
                    setterOrNull,
                    labelGrayColor, inHorizontalLayout), false);
            }
            #endregion

            #region UnityEngine.Object
            if (typeof(Object).IsAssignableFrom(valueType))
            {
                return (UIToolkitObjectFieldEdit(oldElement, label, valueType, (Object)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, foldoutViewKey), false);
            }
            #endregion

            #region AnimationCurve
            if (typeof(AnimationCurve).IsAssignableFrom(valueType) || value is AnimationCurve)
            {
                return (CurveRangeAttributeDrawer.UIToolkitValueEdit(
                    oldElement,
                    label,
                    (AnimationCurve) value,
                    beforeSet,
                    setterOrNull,
                    labelGrayColor,
                    inHorizontalLayout,
                    allAttributes,
                    targets), false);
            }
            #endregion

            #region Hash128
            if (typeof(Hash128).IsAssignableFrom(valueType) || value is Hash128)
            {
                if (oldElement is Hash128Field hash128Field)
                {
                    hash128Field.SetValueWithoutNotify((Hash128)value);
                    return (null, false);
                }
                Hash128Field element = new Hash128Field(label)
                {
                    value = (Hash128)value,
                };

                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(Hash128Field.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }
                return (element, false);
            }
            #endregion

            #region Gradient
            if (typeof(Gradient).IsAssignableFrom(valueType) || value is Gradient)
            {
                if (oldElement is GradientField gradientField)
                {
                    gradientField.SetValueWithoutNotify(value as Gradient);
                    return (null, false);
                }

                GradientField element = new GradientField(label)
                {
                    value = value as Gradient,
                };

                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(GradientField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(evt.newValue);
                    });
                }
                return (element, false);
            }
            #endregion

            #region DateTime
            if (valueType == typeof(DateTime) || value is DateTime || allAttributes.Any(each => each is DateTimeAttribute))
            {
                return (DateTimeAttributeDrawer.UIToolkitValueEdit(
                    oldElement,
                    label,
                    valueType,
                    value,
                    beforeSet,
                    setterOrNull,
                    labelGrayColor,
                    inHorizontalLayout,
                    allAttributes), false);
            }
            #endregion

            #region TimeSpan
            if (valueType == typeof(TimeSpan) || value is TimeSpan || allAttributes.Any(each => each is TimeSpanAttribute))
            {
                return (TimeSpanAttributeDrawer.UIToolkitValueEdit(
                    oldElement,
                    label,
                    valueType,
                    value,
                    beforeSet,
                    setterOrNull,
                    labelGrayColor,
                    inHorizontalLayout,
                    allAttributes), false);
            }
            #endregion

            #region LayerMask
            if (valueType == typeof(LayerMask) || value is LayerMask)
            {
                if (allAttributes.Any(each => each is LayerAttribute))
                {
                    return (LayerAttributeDrawer.UIToolkitValueEditLayerMask(
                        oldElement,
                        label,
                        (LayerMask) value,
                        beforeSet,
                        setterOrNull,
                        labelGrayColor,
                        inHorizontalLayout,
                        allAttributes), false);
                }

                if (oldElement is LayerMaskField lmf)
                {
                    lmf.SetValueWithoutNotify((LayerMask)value);
                    return (null, false);
                }

                LayerMaskField element = new LayerMaskField(label)
                {
                    value = (LayerMask)value,
                };

                if (labelGrayColor)
                {
                    element.labelElement.style.color = AbsRenderer.ReColor;
                }
                if (inHorizontalLayout)
                {
                    element.style.flexDirection = FlexDirection.Column;
                }
                else
                {
                    element.AddToClassList(LayerMaskField.alignedFieldUssClassName);
                }
                if (setterOrNull == null)
                {
                    element.SetEnabled(false);
                    element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
                }
                else
                {
                    element.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    element.RegisterValueChangedCallback(evt =>
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull((LayerMask)evt.newValue);
                    });
                }
                return (element, false);
            }
            #endregion

            #region GUID
            if (valueType == typeof(Guid) || value is Guid)
            {
                return (GuidAttributeDrawer.UIToolkitValueEditGuid(
                    oldElement,
                    label,
                    (Guid) value,
                    beforeSet,
                    setterOrNull,
                    labelGrayColor,
                    inHorizontalLayout,
                    allAttributes), false);
            }
            #endregion

            #region AnimatorState

            if (valueType == typeof(AnimatorState) || value is AnimatorState)
            {
                return (AnimatorStateAttributeDrawer.UIToolkitValueEditAnimatorState(
                    oldElement,
                    allAttributes.OfType<AnimatorStateAttribute>().FirstOrDefault() ?? new AnimatorStateAttribute(),
                    label,
                    (AnimatorState) value,
                    beforeSet,
                    setterOrNull,
                    labelGrayColor,
                    inHorizontalLayout,
                    allAttributes,
                    targets), false);
            }
            if (valueType == typeof(AnimatorStateBase) || value is AnimatorState)
            {
                return (AnimatorStateAttributeDrawer.UIToolkitValueEditAnimatorStateBase(
                    oldElement,
                    allAttributes.OfType<AnimatorStateAttribute>().FirstOrDefault() ?? new AnimatorStateAttribute(),
                    label,
                    (AnimatorStateBase) value,
                    beforeSet,
                    setterOrNull,
                    labelGrayColor,
                    inHorizontalLayout,
                    allAttributes,
                    targets), false);
            }

            #endregion

            #region decimal
            if (value is decimal decimalValue)
            {
                return (SaintsDecimalDrawer.UIToolkitValueEdit(
                    oldElement,
                    label,
                    valueType,
                    decimalValue,
                    beforeSet,
                    setterOrNull,
                    labelGrayColor,
                    inHorizontalLayout,
                    allAttributes), false);
            }
            #endregion


            bool valueIsNull = RuntimeUtil.IsNull(value);

            IEnumerable<Type> genTypes = valueType.GetInterfaces()
                    .Where(each => each.IsGenericType)
                // .Select(each => each.GetGenericTypeDefinition())
                ;
            if (valueType.IsGenericType)
            {
                genTypes = genTypes.Prepend(valueType);
            }

            #region Dictionary
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
                bool isReadOnly = !isNormalDictionary;
                return (SaintsDictionaryDrawer.UIToolkitValueEdit(
                    oldElement as Foldout, label, valueType, value, isReadOnly,
                    dictionaryArgTypes[0], dictionaryArgTypes[1], beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey), false);
            }
            #endregion

            #region List
            if (value is IEnumerable enumerableValue)
            {
                // Debug.Log($"oldElement={oldElement}, {oldElement is Foldout}");
                return (ListDrawerSettingsRenderer.UIToolkitValueEdit(
                        oldElement,
                        label,
                        valueType,
                        enumerableValue,
                        enumerableValue.Cast<object>().ToArray(),
                        beforeSet,
                        setterOrNull,
                        labelGrayColor,
                        inHorizontalLayout,
                        allAttributes,
                        targets,
                        richTextTagProvider,
                        foldoutViewKey
                    ), false);
            }
            #endregion

            // Debug.Log(valueType);

            if (valueType.BaseType == typeof(TypeInfo))  // generic type?
            {
                // EditorGUILayout.TextField(label, value.ToString());
                return (WrapVisualElement(new TextField(label)
                {
                    value = value.ToString(),
                }), false);
            }

            if (valueIsNull)
            {
                if (valueType.IsArray || typeof(IList).IsAssignableFrom(valueType))
                {
                    if (oldElement is LabelButtonField oldLabel && oldLabel.ClassListContains(foldoutViewKey))
                    {
                        return (null, false);
                    }
                    LabelButtonField labelButtonField = new LabelButtonField(label, new Button(() =>
                    {
                        beforeSet?.Invoke(value);
                        object result = valueType.IsArray
                            ? Array.CreateInstance(ReflectUtils.GetElementType(valueType), 0)
                            : Activator.CreateInstance(valueType);
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
                        labelButtonField.labelElement.style.color = AbsRenderer.ReColor;
                    }
                    labelButtonField.AddToClassList(LabelButtonField.alignedFieldUssClassName);
                    labelButtonField.AddToClassList(foldoutViewKey);
                    return (labelButtonField, true);
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
                        textField.labelElement.style.color = AbsRenderer.ReColor;
                    }

                    // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                    if(_nullUss is null)  // bypass life circle
                    {
                        _nullUss = Util.LoadResource<StyleSheet>("UIToolkit/UnityTextInputElementWarning.uss");
                    }
                    textField.styleSheets.Add(_nullUss);

                    return (WrapVisualElement(textField), true);
                }
            }

            // ReSharper disable once InvertIf
            if (oldElement is GeneralTypeEdit gte && gte.ClassListContains(foldoutViewKey))
            {
                gte.CheckRefresh(label, valueType, value, beforeSet, setterOrNull, targets);
                return (null, true);
            }

            // Debug.Log($"recreated! {oldElement}{oldElement?.name}(name={foldoutViewKey})");

            GeneralTypeEdit newCreated = new GeneralTypeEdit(
                label,
                valueType,
                value,
                beforeSet,
                setterOrNull,
                labelGrayColor,
                inHorizontalLayout,
                targets,
                richTextTagProvider,
                foldoutViewKey);
            newCreated.AddToClassList(foldoutViewKey);
            if (ExpandedValue.Contains(foldoutViewKey))
            {
                // Debug.Log($"created expand {foldoutViewKey}");
                // newCreated.value = true;
                // Would you, please, fix your crap, Unity
                newCreated.schedule.Execute(() => newCreated.value = true);
            }

            newCreated.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    // Debug.Log($"add {foldoutViewKey}");
                    ExpandedValue.Add(foldoutViewKey);
                }
                else
                {
                    // Debug.Log($"remove {foldoutViewKey}");
                    ExpandedValue.Remove(foldoutViewKey);
                }
            });

            return (newCreated,
                true);
        }

        private static ObjectField UIToolkitObjectFieldEdit(VisualElement oldElement, string label, Type valueType, Object value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, string foldoutViewKey)
        {
            if (oldElement is ObjectField oldUnityEngineObjectField && oldUnityEngineObjectField.ClassListContains(foldoutViewKey))
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

            element.AddToClassList(foldoutViewKey);

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
                element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
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

        private static VisualElement WrapVisualElement(VisualElement visualElement)
        {
            visualElement.SetEnabled(false);
            visualElement.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
            // visualElement.AddToClassList("unity-base-field__aligned");
            visualElement.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
            return visualElement;
        }

        private class LabelButtonField : BaseField<object>
        {
            public LabelButtonField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static StyleSheet _nullUss;

        public static readonly HashSet<string> ExpandedValue = new HashSet<string>();
    }
}
