using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Spine;
using Spine;
using Spine.Unity;
using UnityEditor;
using Animation = Spine.Animation;

namespace SaintsField.Editor.Drawers.Spine.SpineAnimationPickerDrawer
{
    [CustomPropertyDrawer(typeof(SpineAnimationPickerAttribute))]
    public partial class SpineAnimationPickerAttributeDrawer: SaintsPropertyDrawer
    {
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
                    Type fieldType = ReflectUtils.GetElementType(info.FieldType);
                    Type assetType = typeof(AnimationReferenceAsset);
                    return fieldType == assetType || fieldType.IsSubclassOf(assetType)
                        ? ""
                        : $"Field {fieldType} is not a {assetType}";
                }
                default:
                    return $"Property {property.propertyType} is not a string or AnimationReferenceAsset";
            }
        }

        private struct SpineAnimationInfo: IEquatable<SpineAnimationInfo>
        {
            public SkeletonDataAsset SkeletonDataAsset;
            public string AnimationName;

            public bool Equals(SpineAnimationInfo other)
            {
                return ReferenceEquals(SkeletonDataAsset, other.SkeletonDataAsset) && AnimationName == other.AnimationName;
            }

            public override bool Equals(object obj)
            {
                return obj is SpineAnimationInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(SkeletonDataAsset, AnimationName);
            }

            public override string ToString()
            {
                return $"{AnimationName} ({SkeletonDataAsset.name})";
            }
        }

        private static (bool found, SpineAnimationInfo selectedSpineAnimationInfo) GetSelectedAnimation(SerializedProperty property, SkeletonDataAsset skeletonDataAsset, ExposedList<Animation> animations)
        {
            if(property.propertyType == SerializedPropertyType.String)
            {
                if(animations.Any(each => each.Name == property.stringValue))
                {
                    return (true, new SpineAnimationInfo
                    {
                        SkeletonDataAsset = skeletonDataAsset,
                        AnimationName = property.stringValue,
                    });
                }
                return (false, default);
            }

            AnimationReferenceAsset animationReferenceAsset = property.objectReferenceValue as AnimationReferenceAsset;
            if(animationReferenceAsset == null)
            {
                return (false, default);
            }

            if(animations.Any(each => each.Name == animationReferenceAsset.Animation.Name && ReferenceEquals(skeletonDataAsset, animationReferenceAsset.SkeletonDataAsset)))
            {
                return (true, new SpineAnimationInfo
                {
                    SkeletonDataAsset = skeletonDataAsset,
                    AnimationName = animationReferenceAsset.Animation.Name,
                });
            }
            return (false, default);
        }

        private static AdvancedDropdownMetaInfo GetMetaInfoString(string selectedSpineAnimationInfo, SkeletonDataAsset skeletonDataAsset)
        {
            ExposedList<Animation> animations = skeletonDataAsset.GetAnimationStateData().SkeletonData.Animations;
            AdvancedDropdownList<string> dropdownListValue =
                new AdvancedDropdownList<string>(skeletonDataAsset.name);

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
                new AdvancedDropdownList<AnimationReferenceAsset>(skeletonDataAsset.name);

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
    }
}
