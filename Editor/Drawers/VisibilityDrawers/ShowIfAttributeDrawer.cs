using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer: VisibilityAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(SerializedProperty property, FieldInfo info, object target)
        {
            List<bool> allResults = new List<bool>();

            ShowIfAttribute[] targetAttributes = SerializedUtils.GetAttributesAndDirectParent<ShowIfAttribute>(property).attributes;
            foreach (ShowIfAttribute targetAttribute in targetAttributes)
            {
                (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(targetAttribute.ConditionInfos, property, info, target);

                if (errors.Count > 0)
                {
                    return (string.Join("\n\n", errors), true);
                }

                bool editorModeOk = Util.ConditionEditModeChecker(targetAttribute.EditorMode);
                // And Mode; empty=true, but we won't get empty here
                bool boolResultsOk = boolResults.Append(editorModeOk).All(each => each);
                allResults.Add(boolResultsOk);
            }

            // Or Mode
            bool truly = allResults.Any(each => each);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_READ_ONLY
            UnityEngine.Debug.Log($"{property.name} final={truly}/ars={string.Join(",", allResults)}");
#endif
            return ("", truly);
        }
    }
}
