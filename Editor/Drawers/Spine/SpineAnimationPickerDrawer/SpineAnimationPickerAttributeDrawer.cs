using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Spine;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using Animation = Spine.Animation;

namespace SaintsField.Editor.Drawers.Spine.SpineAnimationPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SpineAnimationPickerAttribute), true)]
    public partial class SpineAnimationPickerAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private const string IconDropdownPath = "Spine/icon-animation-dropdown.png";
        private const string IconPath = "Spine/icon-animation.png";

        private static string GetTypeMismatchError(SerializedProperty property, FieldInfo info)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return "";
                case SerializedPropertyType.ObjectReference:
                {
                    Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
                    Type assetType = typeof(AnimationReferenceAsset);
                    return fieldType == assetType || fieldType.IsSubclassOf(assetType)
                        ? ""
                        : $"Field {fieldType} is not a {assetType}";
                }
                default:
                    return $"Property {property.propertyType} is not a string or AnimationReferenceAsset";
            }
        }

        private static bool GetSelectedAnimation(SerializedProperty property, SkeletonDataAsset skeletonDataAsset)
        {
            ExposedList<Animation> animations = skeletonDataAsset.GetAnimationStateData().SkeletonData.Animations;
            if(property.propertyType == SerializedPropertyType.String)
            {
                return animations.Any(each => each.Name == property.stringValue);
            }

            AnimationReferenceAsset animationReferenceAsset = property.objectReferenceValue as AnimationReferenceAsset;
            if(animationReferenceAsset == null || animationReferenceAsset.Animation == null)
            {
                return false;
            }

            return animations.Any(each =>
                each.Name == animationReferenceAsset.Animation.Name
                && ReferenceEquals(skeletonDataAsset, animationReferenceAsset.SkeletonDataAsset));
        }

        private static AdvancedDropdownMetaInfo GetMetaInfoString(string selectedSpineAnimationInfo, SkeletonDataAsset skeletonDataAsset)
        {
            ExposedList<Animation> animations = skeletonDataAsset.GetAnimationStateData().SkeletonData.Animations;
            AdvancedDropdownList<string> dropdownListValue =
                new AdvancedDropdownList<string>(skeletonDataAsset.name)
                {
                    { "[Null]", null },
                };

            dropdownListValue.AddSeparator();

            List<object> curValues = new List<object>();

            foreach (Animation spineAnimation in animations)
            {
                dropdownListValue.Add(spineAnimation.Name, spineAnimation.Name, icon: IconPath);
                if(spineAnimation.Name == selectedSpineAnimationInfo)
                {
                    curValues.Add(selectedSpineAnimationInfo);
                }
            }

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curSelected;
            if (curValues.Count == 0)
            {
                curSelected = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) =
                    AdvancedDropdownUtil.GetSelected(curValues[0],
                        Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);
                curSelected = stacks;
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = curValues,
                DropdownListValue = dropdownListValue,
                SelectStacks = curSelected,
            };
        }

        private static AdvancedDropdownMetaInfo GetMetaInfoAsset(AnimationReferenceAsset selectedSpineAnimationInfo, SkeletonDataAsset skeletonDataAsset)
        {
            AdvancedDropdownList<AnimationReferenceAsset> dropdownListValue =
                new AdvancedDropdownList<AnimationReferenceAsset>(skeletonDataAsset.name)
                {
                    { "[Null]", null },
                };
            dropdownListValue.AddSeparator();

            List<object> curValues = new List<object>();

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(AnimationReferenceAsset)}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationReferenceAsset animationReferenceAsset = AssetDatabase.LoadAssetAtPath<AnimationReferenceAsset>(path);
                if(animationReferenceAsset != null && animationReferenceAsset.SkeletonDataAsset == skeletonDataAsset)
                {
                    dropdownListValue.Add(animationReferenceAsset.Animation.Name, animationReferenceAsset, icon: IconPath);
                    if (ReferenceEquals(animationReferenceAsset, selectedSpineAnimationInfo))
                    {
                        curValues.Add(animationReferenceAsset);
                    }
                }
            }

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curSelected;
            if (curValues.Count == 0)
            {
                curSelected = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) =
                    AdvancedDropdownUtil.GetSelected(curValues[0],
                        Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);
                curSelected = stacks;
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = curValues,
                DropdownListValue = dropdownListValue,
                SelectStacks = curSelected,
            };
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            SpineAnimationPickerAttribute spineAnimationPickerAttribute = (SpineAnimationPickerAttribute)propertyAttribute;
            (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(spineAnimationPickerAttribute.SkeletonTarget, property, memberInfo, parent);
            if (error != "")
            {
                return new AutoRunnerFixerResult
                {
                    ExecError = error,
                    Error = "",
                };
            }

            bool found = GetSelectedAnimation(property, skeletonDataAsset);
            if (!found)
            {
                return new AutoRunnerFixerResult
                {
                    ExecError = "",
                    Error = $"Animation {(property.propertyType == SerializedPropertyType.String ? property.stringValue: property.objectReferenceValue)} not found in {skeletonDataAsset.name}",
                };
            }

            return null;
        }
    }
}
