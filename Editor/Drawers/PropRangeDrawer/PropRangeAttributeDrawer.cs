using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.UnitDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
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
                minValue = (float)propRangeAttribute.Min;
            }
            else
            {
                (string getError, MemberInfo _, float getValue) =
                    Util.GetOf(propRangeAttribute.MinCallback, 0f, property, info, parentTarget, null);
                error = getError;
                minValue = getValue;
            }

            float maxValue;
            if (propRangeAttribute.MaxCallback == null)
            {
                maxValue = (float)propRangeAttribute.Max;
            }
            else
            {
                (string getError, MemberInfo _, float getValue) =
                    Util.GetOf(propRangeAttribute.MaxCallback, 0f, property, info, parentTarget, null);
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
                Step = (float)propRangeAttribute.Step,
                Error = error,
            };
        }

        public static (string error, int value) GetPreValue(int value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetIntValuePre(value, unitAttribute);
        }
        public static (string error, uint value) GetPreValue(uint value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetUIntValuePre(value, unitAttribute);
        }

        public static (string error, long value) GetPreValue(long value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetLongValuePre(value, unitAttribute);
        }

        public static (string error, ulong value) GetPreValue(ulong value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetULongValuePre(value, unitAttribute);
        }

        public static (string error, double value) GetPreValue(double value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetDoubleValuePre(value, unitAttribute);
        }
        public static (string error, float value) GetPreValue(float value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetFloatValuePre(value, unitAttribute);
        }

        public static (string error, double value) GetPostValue(double value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetDoubleValuePost(value, unitAttribute);
        }
        public static (string error, float value) GetPostValue(float value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetFloatValuePost(value, unitAttribute);
        }

        public static (string error, int value) GetPostValue(int value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetIntValuePost(value, unitAttribute);
        }
        public static (string error, long value) GetPostValue(long value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetLongValuePost(value, unitAttribute);
        }
        public static (string error, ulong value) GetPostValue(ulong value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetULongValuePost(value, unitAttribute);
        }
        public static (string error, uint value) GetPostValue(uint value, UnitAttribute unitAttribute)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (unitAttribute == null)
            {
                return ("", value);
            }

            return UnitAttributeDrawer.GetUIntValuePost(value, unitAttribute);
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
