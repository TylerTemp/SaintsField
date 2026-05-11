using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SortingLayerAttribute), true)]
    public partial class SortingLayerAttributeDrawer : SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (SortingLayer sortingLayer in SortingLayer.layers)
                    {
                        if (sortingLayer.name == property.stringValue)
                        {
                            return null;
                        }
                    }

                    return new AutoRunnerFixerResult
                    {
                        ExecError = "",
                        Error = $"{property.stringValue} not found in layers"
                    };
                }
                case SerializedPropertyType.Integer:
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (SortingLayer sortingLayer in SortingLayer.layers)
                    {
                        if (sortingLayer.id == property.intValue)
                        {
                            return null;
                        }
                    }

                    return new AutoRunnerFixerResult
                    {
                        ExecError = "",
                        Error = $"{property.intValue}(id) not found in layers",
                    };
                }
                default:
                    return new AutoRunnerFixerResult
                    {
                        ExecError = $"{property.propertyType} is not a string or integer",
                        Error = "",
                    };
            }
        }
    }
}
