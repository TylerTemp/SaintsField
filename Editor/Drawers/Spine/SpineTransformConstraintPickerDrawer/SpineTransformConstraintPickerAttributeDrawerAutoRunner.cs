using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Spine;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine.SpineTransformConstraintPickerDrawer
{
    public partial class SpineTransformConstraintPickerAttributeDrawer: IAutoRunnerFixDrawer
    {
        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            if (string.IsNullOrEmpty(property.stringValue))
            {
                return null;
            }

            SpineBonePickerAttribute spineBonePickerAttribute = allAttributes.OfType<SpineBonePickerAttribute>().First();

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

            foreach (TransformConstraintData transformConstraints in GetTransformConstraintData(skeletonData))
            {
                string ikConstraintName = transformConstraints.Name;
                if (ikConstraintName == property.stringValue)
                {
                    return null;
                }

                foundNames.Add(ikConstraintName);
            }

            return new AutoRunnerFixerResult
            {
                CanFix = false,
                ExecError = "",
                Error = $"{property.stringValue} not found. Options are: {string.Join(", ", foundNames)}",
            };
        }

        private static IEnumerable<TransformConstraintData> GetTransformConstraintData(SkeletonData skeletonData)
        {
#if SAINTSFIELD_SPINE_UNITY_4_3_0_OR_NEWER
            return SpineUtils.GetConstraintData<TransformConstraintData>(skeletonData);
#else
            for (int i = 0; i < skeletonData.TransformConstraints.Count; i++)
            {
                yield return skeletonData.TransformConstraints.Items[i];
            }
#endif
        }
    }
}
