using System.Reflection;
using Spine;
using Spine.Unity;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Spine.SpineSkinPickerDrawer
{
    public static class SpineSkinUtils
    {
        public const string IconPath = "Spine/icon-skin.png";

        public static (string error, ExposedList<Skin> skins) GetSkins(string callback, SerializedProperty property, MemberInfo info, object parent)
        {
            (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(callback, property, info, parent);
            if (error != "")
            {
                return (error, null);
            }

            if (skeletonDataAsset == null)
            {
                return ($"No SkeletonDataAsset found for {property.propertyPath}{(callback == null? " ": $" {callback}")}", null);
            }

            SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);

            if (skeletonData == null)
            {
                return ($"No skeletonData found for {property.propertyPath}{(callback == null? " ": $" {callback}")}", null);
            }

            ExposedList<Skin> skins = skeletonData.Skins;
            return ("", skins);
        }
    }
}
