using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer: VisibilityAttributeDrawer
    {
        // and
        protected override (string error, bool shown) IsShown(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info,
            Type type, object target)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)saintsAttribute;

            bool editorModeOk = Util.ConditionEditModeChecker(showIfAttribute.EditorMode);
            if (!editorModeOk)
            {
                return ("", false);
            }

            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(showIfAttribute.ConditionInfos, property, info, target);

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), true);
            }

            // empty = true
            bool truly = boolResults.All(each => each);

            return ("", truly);
        }
    }
}
