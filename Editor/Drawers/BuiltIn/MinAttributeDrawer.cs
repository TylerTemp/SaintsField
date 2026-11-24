using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.BuiltIn
{
    public static class MinAttributeDrawer
    {
        // every numeric type repeats the same guard, so it lives here instead of 10 copies
        private static VisualElement TypeMismatch(VisualElement oldElement, string label, object rawValue, string typeName)
        {
            string message = $"{label}: {rawValue}({rawValue?.GetType().ToString() ?? "null"}) is not {typeName}";
            if (oldElement is HelpBox helpBox)
            {
                UIToolkitUtils.SetHelpBox(helpBox, message);
                return null;
            }

            return new HelpBox(message, HelpBoxMessageType.Error);
        }

        public static VisualElement UIToolkitValueEditSByte(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not sbyte value)
            {
                return TypeMismatch(oldElement, label, rawValue, "a sbyte");
            }

            if (oldElement is IntegerField oldIntegerField)
            {
                oldIntegerField.SetValueWithoutNotify(value);
                return null;
            }

            int min = Mathf.Max((int)minAttribute.min, sbyte.MinValue);

            IntegerField field = new IntegerField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    // the editing widget is wider than the member, so clamp to both the min and the type
                    sbyte newValue = (sbyte)Mathf.Clamp(evt.newValue, min, sbyte.MaxValue);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
        }

        public static VisualElement UIToolkitValueEditByte(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not byte value)
            {
                return TypeMismatch(oldElement, label, rawValue, "a byte");
            }

            if (oldElement is IntegerField oldIntegerField)
            {
                oldIntegerField.SetValueWithoutNotify(value);
                return null;
            }

            int min = Mathf.Max((int)minAttribute.min, byte.MinValue);

            IntegerField field = new IntegerField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    byte newValue = (byte)Mathf.Clamp(evt.newValue, min, byte.MaxValue);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
        }

        public static VisualElement UIToolkitValueEditShort(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not short value)
            {
                return TypeMismatch(oldElement, label, rawValue, "a short");
            }

            if (oldElement is IntegerField oldIntegerField)
            {
                oldIntegerField.SetValueWithoutNotify(value);
                return null;
            }

            int min = Mathf.Max((int)minAttribute.min, short.MinValue);

            IntegerField field = new IntegerField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    short newValue = (short)Mathf.Clamp(evt.newValue, min, short.MaxValue);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
        }

        public static VisualElement UIToolkitValueEditUShort(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not ushort value)
            {
                return TypeMismatch(oldElement, label, rawValue, "a ushort");
            }

            if (oldElement is IntegerField oldIntegerField)
            {
                oldIntegerField.SetValueWithoutNotify(value);
                return null;
            }

            int min = Mathf.Max((int)minAttribute.min, ushort.MinValue);

            IntegerField field = new IntegerField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    ushort newValue = (ushort)Mathf.Clamp(evt.newValue, min, ushort.MaxValue);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
        }

        public static VisualElement UIToolkitValueEditInt(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not int value)
            {
                return TypeMismatch(oldElement, label, rawValue, "an int");
            }

            if (oldElement is IntegerField oldIntegerField)
            {
                oldIntegerField.SetValueWithoutNotify(value);
                return null;
            }

            int min = (int)minAttribute.min;

            IntegerField field = new IntegerField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    int newValue = Mathf.Max(evt.newValue, min);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
        }

        public static VisualElement UIToolkitValueEditUInt(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not uint value)
            {
                return TypeMismatch(oldElement, label, rawValue, "an uint");
            }

            // a negative min can never bind an uint, so it collapses to 0
            uint min = minAttribute.min <= 0f ? uint.MinValue : (uint)minAttribute.min;

#if UNITY_2022_3_OR_NEWER
            if (oldElement is UnsignedIntegerField oldUnsignedIntegerField)
            {
                oldUnsignedIntegerField.SetValueWithoutNotify(value);
                return null;
            }

            UnsignedIntegerField field = new UnsignedIntegerField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    uint newValue = Math.Max(evt.newValue, min);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
#else
            if (oldElement is IntegerField oldIntegerField)
            {
                oldIntegerField.SetValueWithoutNotify((int)value);
                return null;
            }

            IntegerField field = new IntegerField(label)
            {
                value = (int)value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    uint newValue = evt.newValue <= 0 ? min : Math.Max((uint)evt.newValue, min);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify((int)newValue);
                    }
                });
            }

            return field;
#endif
        }

        public static VisualElement UIToolkitValueEditLong(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not long value)
            {
                return TypeMismatch(oldElement, label, rawValue, "a long");
            }

            if (oldElement is LongField oldLongField)
            {
                oldLongField.SetValueWithoutNotify(value);
                return null;
            }

            long min = (long)minAttribute.min;

            LongField field = new LongField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    long newValue = Math.Max(evt.newValue, min);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
        }

        public static VisualElement UIToolkitValueEditULong(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not ulong value)
            {
                return TypeMismatch(oldElement, label, rawValue, "an ulong");
            }

            // a negative min can never bind an ulong, so it collapses to 0
            ulong min = minAttribute.min <= 0f ? ulong.MinValue : (ulong)minAttribute.min;

#if UNITY_2022_3_OR_NEWER
            if (oldElement is UnsignedLongField oldUnsignedLongField)
            {
                oldUnsignedLongField.SetValueWithoutNotify(value);
                return null;
            }

            UnsignedLongField field = new UnsignedLongField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    ulong newValue = Math.Max(evt.newValue, min);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
#else
            if (oldElement is LongField oldLongField)
            {
                oldLongField.SetValueWithoutNotify((long)value);
                return null;
            }

            LongField field = new LongField(label)
            {
                value = (long)value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    ulong newValue = evt.newValue <= 0 ? min : Math.Max((ulong)evt.newValue, min);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    if (newValue != (ulong)evt.newValue)
                    {
                        field.SetValueWithoutNotify((long)newValue);
                    }
                });
            }

            return field;
#endif
        }

        public static VisualElement UIToolkitValueEditFloat(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not float value)
            {
                return TypeMismatch(oldElement, label, rawValue, "a float");
            }

            if (oldElement is FloatField oldFloatField)
            {
                oldFloatField.SetValueWithoutNotify(value);
                return null;
            }

            float min = minAttribute.min;

            FloatField field = new FloatField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    float newValue = Mathf.Max(evt.newValue, min);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
        }

        public static VisualElement UIToolkitValueEditDouble(VisualElement oldElement, MinAttribute minAttribute, string label, object rawValue, Type valueType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (rawValue is not double value)
            {
                return TypeMismatch(oldElement, label, rawValue, "a double");
            }

            if (oldElement is DoubleField oldDoubleField)
            {
                oldDoubleField.SetValueWithoutNotify(value);
                return null;
            }

            double min = minAttribute.min;

            DoubleField field = new DoubleField(label)
            {
                value = value,
            };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull != null,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    double newValue = Math.Max(evt.newValue, min);
                    beforeSet?.Invoke(evt.previousValue);
                    setterOrNull(newValue);
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (newValue != evt.newValue)
                    {
                        field.SetValueWithoutNotify(newValue);
                    }
                });
            }

            return field;
        }
    }
}
