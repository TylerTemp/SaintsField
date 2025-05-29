using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Condition;
using SaintsField.Editor.Drawers.VisibilityDrawers.VisibilityDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
    public class ShowIfAttributeDrawer: VisibilityAttributeDrawer
    {
        // protected override (string error, bool shown) IsShown(ShowIfAttribute targetAttribute, SerializedProperty property, FieldInfo info, object target)
        // {
        //     return HelperShowIfIsShown(targetAttribute.ConditionInfos, property, info, target);
        // }

        public static (string error, bool shown) HelperShowIfIsShown(IEnumerable<ConditionInfo> conditionInfos,
            SerializedProperty property, MemberInfo info, object target)
        {
            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(conditionInfos, property, info, target);

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), true);
            }

            // and; empty = true, thus empty=show
            return ("",  boolResults.All(each => each));
        }
    }
}
