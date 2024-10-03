using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Condition;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer: VisibilityAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(ShowIfAttribute targetAttribute, SerializedProperty property, FieldInfo info, object target)
        {
            return HelperShowIfIsShown(targetAttribute.ConditionInfos, targetAttribute.EditorMode, property, info, target);
        }

        public static (string error, bool shown) HelperShowIfIsShown(IEnumerable<ConditionInfo> conditionInfos, EMode editorMode, SerializedProperty property, FieldInfo info, object target)
        {
            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(conditionInfos, property, info, target);

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), true);
            }

            bool editorModeOk = Util.ConditionEditModeChecker(editorMode);
            // and; empty = true, thus empty=show
            return ("",  boolResults.Prepend(editorModeOk).All(each => each));
        }
    }
}
