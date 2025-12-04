using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Spine;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine.SpineEventPickerDrawer
{
    public partial class SpineEventPickerAttributeDrawer: IAutoRunnerFixDrawer
    {
        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            if (string.IsNullOrEmpty(property.stringValue))
            {
                return null;
            }

            SpineEventPickerAttribute spineBonePickerAttribute = allAttributes.OfType<SpineEventPickerAttribute>().First();

            (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(spineBonePickerAttribute.SkeletonTarget, property, memberInfo,
                parent);
            if (error != "")
            {
                return new AutoRunnerFixerResult
                {
                    CanFix = false,
                    Error = "",
                    ExecError = error,
                };
            }

            SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
            if (skeletonData == null)
            {
                return new AutoRunnerFixerResult
                {
                    CanFix = false,
                    Error = "",
                    ExecError = error,
                };
            }

            HashSet<string> foundNames = new HashSet<string>();
            foreach (EventData skeletonDataEvent in skeletonData.Events)
            {
                string eventName = skeletonDataEvent.Name;
                if (eventName == property.stringValue)
                {
                    return null;
                }

                foundNames.Add(eventName);
            }

            return new AutoRunnerFixerResult
            {
                CanFix = false,
                ExecError = "",
                Error = $"{property.stringValue} not found. Options are: {string.Join(", ", foundNames)}",
            };
        }
    }
}
