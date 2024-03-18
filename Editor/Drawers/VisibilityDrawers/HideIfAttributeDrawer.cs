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
            EMode editorMode = hideIfAttribute.EditorMode;
            bool editorRequiresEdit = editorMode.HasFlag(EMode.Edit);
            bool editorRequiresPlay = editorMode.HasFlag(EMode.Play);

            bool editorModeIsTrue = (
                !editorRequiresEdit || !EditorApplication.isPlaying
            ) && (
                !editorRequiresPlay || EditorApplication.isPlaying
            );

            List<bool> callbackTruly = new List<bool>();
            List<string> errors = new List<string>();

            if (!(editorRequiresEdit && editorRequiresPlay))
            {
                callbackTruly.Add(editorModeIsTrue);
            }

            foreach (string andCallback in hideIfAttribute.Callbacks)
            {
                (string error, bool isTruly) = Util.GetTruly(target, andCallback);
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

            // or, get hide
            // any(empty) = false, reversed=show(true)
            // we need to manually deal the empty
            if(callbackTruly.Count == 0)
            {
                return ("", false);  // don't show if empty
            }

            bool truly = callbackTruly.Any(each => each);

            // reverse, get shown
            return ("", !truly);
        }
    }
}
