using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdaptDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(PropRangeAttribute), true)]
    public partial class PropRangeAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
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
            MemberInfo info, object parentTarget)
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

        private static (string error, double value) GetPreValue(double value, AdaptAttribute adaptAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (adaptAttribute == null)
            {
                return ("", value);
            }

            return AdaptAttributeDrawer.GetDoubleValuePre(value);
        }

        private static (string error, double value) GetPostValue(double value, AdaptAttribute adaptAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (adaptAttribute == null)
            {
                return ("", value);
            }

            return AdaptAttributeDrawer.GetDoubleValuePost(value);
        }

        private static float GetValue(MetaInfo metaInfo, float newValue)
        {
            // property.floatValue = newValue;
            float step = metaInfo.Step;
            bool isFloat = metaInfo.IsFloat;
            // Debug.Log(step);
            if (step <= 0)
            {
                // return newValue;
                return Mathf.Clamp(newValue, metaInfo.MinValue, metaInfo.MaxValue);
            }

            if (isFloat)
            {
                return Util.BoundFloatStep(newValue, metaInfo.MinValue, metaInfo.MaxValue, step);
            }

            return Util.BoundIntStep(newValue, metaInfo.MinValue, metaInfo.MaxValue, (int)step);
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, (PropRangeAttribute)propertyAttribute, memberInfo, parent);
            float curPropValue = metaInfo.IsFloat ? property.floatValue : property.intValue;
            float parsedValue = GetValue(metaInfo, curPropValue);
            if (Mathf.Approximately(curPropValue, parsedValue))
            {
                return null;
            }

            return new AutoRunnerFixerResult
            {
                ExecError = "",
                Error = $"Expect {parsedValue}, but got {curPropValue}",
                CanFix = true,
                Callback = () =>
                {
                    if (metaInfo.IsFloat)
                    {
                        property.doubleValue = parsedValue;
                    }
                    else
                    {
                        property.intValue = (int)parsedValue;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                },
            };

        }
    }
}
