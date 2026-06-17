using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsDecimal), true)]
    public partial class SaintsDecimalDrawer: SaintsPropertyDrawer
    {
#if !SAINTSFIELD_UI_TOOLKIT_DISABLE
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            SaintsDecimalField field = new SaintsDecimalField(GetPreferredLabel(property));
            field.DecimalTextField.AddToClassList(DecimalTextField.alignedFieldUssClassName);
            EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(field);
            return emptyPrefabOverrideElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsDecimalField field = container.Q<SaintsDecimalField>();
            // int propIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            field.ManuallyBindProperty(property, newValue =>
            {
                string error = UpdateCachedDecimalValue(property, info, newValue);
                if (error != "")
                {
                    Debug.LogError(error);
                }
            });

            AddContextualMenuManipulator(field, property);
        }

#endif

        private static void AddContextualMenuManipulator(SaintsDecimalFieldAbs field, SerializedProperty property)
        {
            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => {});

            field.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction($"Copy \"{field.value}\"", _ =>
                {
                    EditorGUIUtility.systemCopyBuffer = $"{field.value}";
                });

                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (decimal.TryParse(clipboardText, out decimal value))
                {
                    evt.menu.AppendAction($"Paste \"{clipboardText}\"", _ =>
                    {
                        field.value = value;
                    });
                }
            }));
        }

        internal readonly struct DecimalPropertyInfo
        {
            public readonly string Error;
            public readonly SerializedProperty LoProp;
            public readonly SerializedProperty MidProp;
            public readonly SerializedProperty HiProp;
            public readonly SerializedProperty FlagsProp;

            public DecimalPropertyInfo(string error, SerializedProperty loProp, SerializedProperty midProp,
                SerializedProperty hiProp, SerializedProperty flagsProp)
            {
                Error = error;
                LoProp = loProp;
                MidProp = midProp;
                HiProp = hiProp;
                FlagsProp = flagsProp;
            }
        }

        internal static DecimalPropertyInfo GetDecimalPropertyInfo(SerializedProperty property)
        {
            SerializedProperty loProp = property.FindPropertyRelative(nameof(SaintsDecimal.lo));
            SerializedProperty midProp = property.FindPropertyRelative(nameof(SaintsDecimal.mid));
            SerializedProperty hiProp = property.FindPropertyRelative(nameof(SaintsDecimal.hi));
            SerializedProperty flagsProp = property.FindPropertyRelative(nameof(SaintsDecimal.flags));

            return ValidateDecimalPropertyInfo(property, loProp, midProp, hiProp, flagsProp);
        }

        internal static bool IsSerializedActualDecimal(SerializedProperty property)
        {
            SerializedProperty propertyType = property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType));
            return propertyType != null && (SaintsPropertyType)propertyType.intValue == SaintsPropertyType.Decimal;
        }

        internal static DecimalPropertyInfo GetSerializedActualDecimalPropertyInfo(SerializedProperty property)
        {
            SerializedProperty intValuesProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.intValues));
            if (intValuesProp == null || !intValuesProp.isArray || intValuesProp.arraySize < 4)
            {
                return new DecimalPropertyInfo(
                    $"{nameof(SaintsSerializedProperty.intValues)}[0..3] not found in {property.propertyPath}",
                    null, null, null, null);
            }

            SerializedProperty loProp = intValuesProp.GetArrayElementAtIndex(0);
            SerializedProperty midProp = intValuesProp.GetArrayElementAtIndex(1);
            SerializedProperty hiProp = intValuesProp.GetArrayElementAtIndex(2);
            SerializedProperty flagsProp = intValuesProp.GetArrayElementAtIndex(3);

            return ValidateDecimalPropertyInfo(property, loProp, midProp, hiProp, flagsProp);
        }

        private static DecimalPropertyInfo ValidateDecimalPropertyInfo(SerializedProperty property,
            SerializedProperty loProp, SerializedProperty midProp, SerializedProperty hiProp,
            SerializedProperty flagsProp)
        {
            if (loProp == null)
            {
                return new DecimalPropertyInfo($"{nameof(SaintsDecimal.lo)} not found in {property.propertyPath}",
                    null, null, null, null);
            }

            if (midProp == null)
            {
                return new DecimalPropertyInfo($"{nameof(SaintsDecimal.mid)} not found in {property.propertyPath}",
                    loProp, null, null, null);
            }

            if (hiProp == null)
            {
                return new DecimalPropertyInfo($"{nameof(SaintsDecimal.hi)} not found in {property.propertyPath}",
                    loProp, midProp, null, null);
            }

            if (flagsProp == null)
            {
                return new DecimalPropertyInfo($"{nameof(SaintsDecimal.flags)} not found in {property.propertyPath}",
                    loProp, midProp, hiProp, null);
            }

            return new DecimalPropertyInfo("", loProp, midProp, hiProp, flagsProp);
        }

        internal static decimal GetDecimalValue(DecimalPropertyInfo propertyInfo)
        {
            int flags = propertyInfo.FlagsProp.intValue;
            return new decimal(
                propertyInfo.LoProp.intValue,
                propertyInfo.MidProp.intValue,
                propertyInfo.HiProp.intValue,
                (flags & unchecked((int)0x80000000)) != 0,
                (byte)((flags >> 16) & 0x7F));
        }

        internal static bool SetDecimalValue(DecimalPropertyInfo propertyInfo, decimal value)
        {
            bool changed = SetDecimalValueWithoutApply(propertyInfo, value);
            if (changed)
            {
                propertyInfo.FlagsProp.serializedObject.ApplyModifiedProperties();
            }

            return changed;
        }

        internal static bool SetDecimalValueWithoutApply(DecimalPropertyInfo propertyInfo, decimal value)
        {
            int[] bits = decimal.GetBits(value);
            bool changed = false;
            changed |= UpdateIntValue(propertyInfo.LoProp, bits[0]);
            changed |= UpdateIntValue(propertyInfo.MidProp, bits[1]);
            changed |= UpdateIntValue(propertyInfo.HiProp, bits[2]);
            changed |= UpdateIntValue(propertyInfo.FlagsProp, bits[3]);
            return changed;
        }

        internal static bool UpdateIntValue(SerializedProperty property, int newValue)
        {
            if (property.intValue == newValue)
            {
                return false;
            }

            property.intValue = newValue;
            return true;
        }

        internal static string UpdateCachedDecimalValue(SerializedProperty property, FieldInfo info, decimal newValue)
        {
            object newParent = SerializedUtils.GetAttributesAndDirectParent<Attribute>(property).parent;
            (string error, int _, object thisData) = Util.GetValue(property, info, newParent);
            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            FieldInfo valueField = typeof(SaintsDecimal).GetField(nameof(SaintsDecimal.value),
                BindingFlags.Public | BindingFlags.Instance);
            FieldInfo cacheField = typeof(SaintsDecimal).GetField(nameof(SaintsDecimal.cached),
                BindingFlags.Public | BindingFlags.Instance);
            if (valueField == null || cacheField == null)
            {
                return $"{nameof(SaintsDecimal)} cache fields not found";
            }

            decimal curCachedValue = (decimal)valueField.GetValue(thisData);
            if (curCachedValue != newValue)
            {
                valueField.SetValue(thisData, newValue);
                cacheField.SetValue(thisData, true);
            }

            return "";
        }
    }
}
