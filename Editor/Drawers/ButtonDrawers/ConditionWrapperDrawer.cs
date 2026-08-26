using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ButtonDrawers
{
    // this is a workaround... this ensures the decorated attribute's order won't get above these
    // this is a hack of the GetAttributes cache that secretly changed SaintsField attribute to top
    [CustomPropertyDrawer(typeof(DecButtonShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(AboveButtonShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(AboveButtonHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowButtonShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowButtonHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(PostFieldButtonShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(PostFieldButtonHideIfAttribute), true)]

    [CustomPropertyDrawer(typeof(PositionHandleShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(PositionHandleHideIfAttribute), true)]

    [CustomPropertyDrawer(typeof(DrawLabelShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLabelHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(RadiusHandleShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(RadiusHandleHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(RotationHandleShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(RotationHandleHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(ScaleHandleShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(ScaleHandleHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(SliderHandleShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(SliderHandleHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(PrimitiveBoundsHandleShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(PrimitiveBoundsHandleHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(SphereHandleCapShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(SphereHandleCapHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawWireDiscShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawWireDiscHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(ArrowHandleCapShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(ArrowHandleCapHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineFromShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineFromHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineToShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineToHideIfAttribute), true)]
    [CustomPropertyDrawer(typeof(SaintsArrowShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(SaintsArrowHideIfAttribute), true)]

    [CustomPropertyDrawer(typeof(DecButtonDisableIfAttribute), true)]
    [CustomPropertyDrawer(typeof(AboveButtonDisableIfAttribute), true)]
    [CustomPropertyDrawer(typeof(AboveButtonEnableIfAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowButtonDisableIfAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowButtonEnableIfAttribute), true)]
    [CustomPropertyDrawer(typeof(PostFieldButtonDisableIfAttribute), true)]
    [CustomPropertyDrawer(typeof(PostFieldButtonEnableIfAttribute), true)]
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    public class ConditionWrapperDrawer: SaintsPropertyDrawer
    {

    }
}
