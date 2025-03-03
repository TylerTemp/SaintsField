using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(PropRangeAttribute), true)]
    public partial class PropRangeAttributeDrawer: SaintsPropertyDrawer
    {
        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public bool IsFloat;
            public float MinValue;
            public float MaxValue;
            public float Step;

            public string Error;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parentTarget)
        {
            PropRangeAttribute propRangeAttribute = (PropRangeAttribute)saintsAttribute;

            bool isFloat = property.propertyType == SerializedPropertyType.Float;
            // object parentTarget = GetParentTarget(property);
            string error = "";

            float minValue;
            if (propRangeAttribute.MinCallback == null)
            {
                minValue = propRangeAttribute.Min;
            }
            else
            {
                (string getError, float getValue) =
                    Util.GetOf(propRangeAttribute.MinCallback, 0f, property, info, parentTarget);
                error = getError;
                minValue = getValue;
            }

            float maxValue;
            if (propRangeAttribute.MaxCallback == null)
            {
                maxValue = propRangeAttribute.Max;
            }
            else
            {
                (string getError, float getValue) =
                    Util.GetOf(propRangeAttribute.MaxCallback, 0f, property, info, parentTarget);
                error = getError;
                maxValue = getValue;
            }

            if (error != "")
            {
                return new MetaInfo
                {
                    IsFloat = isFloat,
                    Error = error,
                };
            }

            if (maxValue < minValue)
            {
                return new MetaInfo
                {
                    IsFloat = isFloat,
                    Error = $"max({maxValue}) should be greater than min({minValue})",
                };
            }

            return new MetaInfo
            {
                IsFloat = isFloat,
                MinValue = minValue,
                MaxValue = maxValue,
                Step = propRangeAttribute.Step,
                Error = error,
            };
        }
    }
}
