using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Condition;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfAttributeDrawer: ShowIfAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(ShowIfAttribute targetAttribute, SerializedProperty property, FieldInfo info, object target)
        {
            return HelperHideIfIsShown(targetAttribute.ConditionInfos, targetAttribute.EditorMode, property, info, target);
        }

        public static (string error, bool shown) HelperHideIfIsShown(IEnumerable<ConditionInfo> conditionInfos, EMode editorMode, SerializedProperty property, FieldInfo info, object target)
        {
            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(conditionInfos, property, info, target);

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), true);
            }

            // editor || or, Any(empty)=false=!hide=show. But because in ShowIf, empty=true=show, so we need to negate it.
            if (editorMode == 0 && boolResults.Count == 0)
            {
                return ("", false);  // don't show
            }

            // this will be false if user does not pass it.
            bool editorModeOk = Util.ConditionEditModeChecker(editorMode);
            bool isHidden = boolResults.Prepend(editorModeOk).Any(each => each);
            return ("",  !isHidden);
        }
    }
}
