using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Condition;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfAttributeDrawer: ShowIfAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(ShowIfAttribute targetAttribute, SerializedProperty property, FieldInfo info, object target)
        {
            return HelperHideIfIsShown(targetAttribute.ConditionInfos, property, info, target);
        }

        public static (string error, bool shown) HelperHideIfIsShown(IEnumerable<ConditionInfo> conditionInfos,
            SerializedProperty property, FieldInfo info, object target)
        {
            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(conditionInfos, property, info, target);

            // Debug.Log($"{string.Join("\n\n", errors)}/{string.Join(",", boolResults)}/{string.Join(",", conditionInfos)}");

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), true);
            }

            // Any(empty)=false=!hide=show. But because in ShowIf, empty=true=show, so we need to negate it.
            if (boolResults.Count == 0)
            {
                return ("", false);  // don't show
            }

            // this will be false if user does not pass it.
            bool isHidden = boolResults.Any(each => each);
            return ("",  !isHidden);
        }
    }
}
