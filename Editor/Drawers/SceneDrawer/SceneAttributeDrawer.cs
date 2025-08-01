using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SceneAttribute), true)]
    public partial class SceneAttributeDrawer : SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {

            string[] scenes = SceneUtils.GetTrimedScenePath(((SceneAttribute)propertyAttribute).FullPath).ToArray();

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    if(Array.IndexOf(scenes, property.stringValue) == -1)
                    {
                        return new AutoRunnerFixerResult
                        {
                            ExecError = "",
                            Error = $"{property.name} must be a scene in build settings, get {property.stringValue}",
                        };
                    }

                    return null;
                }
                case SerializedPropertyType.Integer:
                {
                    if (property.intValue < 0 || property.intValue >= scenes.Length)
                    {
                        return new AutoRunnerFixerResult
                        {
                            ExecError = "",
                            Error = $"{property.name} must be a scene in build settings (${scenes.Length}), get {property.intValue}",
                        };
                    }

                    return null;
                }
                default:
                    return new AutoRunnerFixerResult
                    {
                        ExecError = "",
                        Error = $"{property.name} must be an int or a string, get {property.propertyType}",
                    };
            }
        }
    }
}
