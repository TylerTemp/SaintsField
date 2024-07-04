using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfAttributeDrawer: ShowIfAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info,
            Type type, object target)
        {
            HideIfAttribute hideIfAttribute = (HideIfAttribute)saintsAttribute;

            bool editorModeOk = Util.ConditionEditModeChecker(hideIfAttribute.EditorMode);
            if (!editorModeOk)
            {
                return ("", false);
            }

            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(hideIfAttribute.ConditionInfos, property, info, target);

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), true);
            }

            // or, get hide
            // any(empty) = false, reversed=show(true)
            // we need to manually deal the empty
            if(boolResults.Count == 0)
            {
                return ("", false);  // don't show if empty
            }

            bool truly = boolResults.Any(each => each);

            // reverse, get shown
            return ("", !truly);
        }
    }
}
