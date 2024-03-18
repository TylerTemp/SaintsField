using System;
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
        // and
        protected override (string error, bool shown) IsShown(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info,
            Type type, object target)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)saintsAttribute;

            EMode editorMode = showIfAttribute.EditorMode;
            bool editorRequiresEdit = editorMode.HasFlag(EMode.Edit);
            bool editorRequiresPlay = editorMode.HasFlag(EMode.Play);

            bool editorModeIsTrue = (
                !editorRequiresEdit || !EditorApplication.isPlaying
            ) && (
                !editorRequiresPlay || EditorApplication.isPlaying
            );

            List<bool> callbackTruly = new List<bool>();
            List<string> errors = new List<string>();
            if(!(editorRequiresEdit && editorRequiresPlay))
            {
                callbackTruly.Add(editorModeIsTrue);
            }

            foreach (string andCallback in showIfAttribute.Callbacks)
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

            // empty = true
            bool truly = callbackTruly.All(each => each);

            return ("", truly);
        }
    }
}
