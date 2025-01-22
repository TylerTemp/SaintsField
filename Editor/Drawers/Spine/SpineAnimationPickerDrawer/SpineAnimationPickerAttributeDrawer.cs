using SaintsField.Editor.Core;
using SaintsField.Spine;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Spine.SpineAnimationPickerDrawer
{
    [CustomPropertyDrawer(typeof(SpineAnimationPickerAttribute))]
    public partial class SpineAnimationPickerAttributeDrawer: SaintsPropertyDrawer
    {
        private static string GetTypeMismatchError(SerializedProperty property)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return "";
                case SerializedPropertyType.ObjectReference:
                {
                    if(property.FindPropertyRelative("skeletonDataAsset") == null || property.FindPropertyRelative("animationName") == null)
                    {
                        return "Object is not a AnimationReferenceAsset";
                    }
                    return "";
                }
                default:
                    return $"Property {property.propertyType} is not a string or AnimationReferenceAsset";
            }
        }
    }
}
