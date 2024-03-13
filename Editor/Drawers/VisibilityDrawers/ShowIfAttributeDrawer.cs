using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer: VisibilityAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info,
            Type type, object target)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)saintsAttribute;

            List<bool> callbackTruly = new List<bool>();
            List<string> errors = new List<string>();

            foreach (string andCallback in showIfAttribute.orCallbacks)
            {
                (string error, bool isTruly) = IsTruly(target, type, andCallback);
                if (error != "")
                {
                    errors.Add(error);
                }
                callbackTruly.Add(isTruly);
            }

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), true);
            }

            // empty means show
            if(callbackTruly.Count == 0)
            {
                return ("", true);
            }

            bool truly = callbackTruly.All(each => each);

            return ("", truly);
        }
    }
}
