using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.Drawers.DisabledDrawers
{
    [CustomPropertyDrawer(typeof(EnableIfAttribute))]
    public class EnableIfAttributeDrawer: ReadOnlyAttributeDrawer
    {
        protected override (string error, bool disabled) IsDisabled(SerializedProperty property,
            ReadOnlyAttribute targetAttribute, FieldInfo info, Type type, object target)
        {
            string[] bys = targetAttribute.ReadOnlyBys;
            if(bys is null)
            {
                return ("", targetAttribute.readOnlyDirectValue);
            }

            List<bool> callbackTruly = new List<bool>();
            List<string> errors = new List<string>();

            foreach (string andCallback in bys)
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

            // empty means hide
            if (callbackTruly.Count == 0)
            {
                return ("", targetAttribute.readOnlyDirectValue);
            }

            // or, get enabled
            bool truly = callbackTruly.Any(each => each);

            // reverse, get disabled
            return ("", !truly);
        }
    }
}
