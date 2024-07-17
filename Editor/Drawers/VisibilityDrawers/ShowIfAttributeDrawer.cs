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
            foreach (var targetAttribute in targetAttributes)
            {
                (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(targetAttribute.ConditionInfos, property, info, target);

                if (errors.Count > 0)
                {
                    return (string.Join("\n\n", errors), true);
                }
                
                bool editorModeOk = Util.ConditionEditModeChecker(targetAttribute.EditorMode);
                // empty = true
                bool boolResultsOk = boolResults.All(each => each);
                allResults.Add(editorModeOk && boolResultsOk);
            }
            
            // Or/And Mode
            bool truly = targetAttributes.Length > 1 ? allResults.Any(each => each) : allResults.All(each => each);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_READ_ONLY
            UnityEngine.Debug.Log($"{property.name} final={truly}/ars={string.Join(",", allResults)}");
#endif
            return ("", truly);
        }
    }
}
