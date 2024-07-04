using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.DisabledDrawers
{
    [CustomPropertyDrawer(typeof(EnableIfAttribute))]
    public class EnableIfAttributeDrawer: ReadOnlyAttributeDrawer
    {
        protected override (string error, bool disabled) IsDisabled(SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object target)
        {
            EnableIfAttribute targetAttribute = (EnableIfAttribute) saintsAttribute;

            bool editorModeOk = Util.ConditionEditModeChecker(targetAttribute.EditorMode);
            if (!editorModeOk)
            {
                return ("", false);
            }

            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(targetAttribute.ConditionInfos, property, info, target);

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), false);
            }

            // or, get enabled
            bool truly = boolResults.Any(each => each);

            // reverse, get disabled
            return ("", !truly);
        }
    }
}
