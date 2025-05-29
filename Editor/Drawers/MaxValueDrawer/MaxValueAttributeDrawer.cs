using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.MaxValueDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(MaxValueAttribute), true)]
    public partial class MaxValueAttributeDrawer : SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static (string error, float valueLimit) GetLimitFloat(SerializedProperty property, MaxValueAttribute maxValueAttribute, MemberInfo info, object parentTarget)
        {
            return maxValueAttribute.ValueCallback == null
                ? ("", maxValueAttribute.Value)
                : Util.GetOf(maxValueAttribute.ValueCallback, 0f, property, info, parentTarget);
        }

        public AutoRunnerFixerResult AutoRunFix(UnityEngine.PropertyAttribute propertyAttribute, IReadOnlyList<UnityEngine.PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {

            if (property.isArray)
            {
                return null;
            }

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                {
                    MaxValueAttribute maxValueAttribute = (MaxValueAttribute) propertyAttribute;
                    (string error, float valueLimit) = GetLimitFloat(property, maxValueAttribute, memberInfo, parent);
                    if (error != "")
                    {
                        return new AutoRunnerFixerResult
                        {
                            ExecError = error,
                            Error = "",
                        };
                    }
                    if (property.floatValue > valueLimit)
                    {
                        return new AutoRunnerFixerResult
                        {
                            ExecError = "",
                            Error = $"Expected value <= {valueLimit}, get {property.floatValue}",
                            CanFix = true,
                            Callback = () =>
                            {
                                property.floatValue = valueLimit;
                                property.serializedObject.ApplyModifiedProperties();
                            },
                        };
                    }

                    return null;
                }
                case SerializedPropertyType.Integer:
                {
                    MaxValueAttribute maxValueAttribute = (MaxValueAttribute) propertyAttribute;
                    (string error, float valueLimit) = GetLimitFloat(property, maxValueAttribute, memberInfo, parent);
                    if (error != "")
                    {
                        return new AutoRunnerFixerResult
                        {
                            ExecError = error,
                            Error = "",
                        };
                    }

                    if (property.intValue > valueLimit)
                    {
                        return new AutoRunnerFixerResult
                        {
                            ExecError = "",
                            Error = $"Expected value <= {valueLimit}, get {property.intValue}",
                            CanFix = true,
                            Callback = () =>
                            {
                                property.intValue = (int)valueLimit;
                                property.serializedObject.ApplyModifiedProperties();
                            }
                        };
                    }

                    return null;
                }
                default:
                    return new AutoRunnerFixerResult
                    {
                        ExecError = "",
                        Error = $"Unsupported type {property.propertyType} for {property.propertyPath}",
                    };
            }
        }
    }
}
