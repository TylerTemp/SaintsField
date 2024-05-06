using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.DisabledDrawers
{
    [CustomPropertyDrawer(typeof(EnableIfAttribute))]
    public class EnableIfAttributeDrawer: ReadOnlyAttributeDrawer
    {
        protected override (string error, bool disabled) IsDisabled(SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, Type type, object target)
        {
            EnableIfAttribute targetAttribute = (EnableIfAttribute) saintsAttribute;

            EMode editorMode = targetAttribute.EditorMode;
            bool editorRequiresEdit = editorMode.HasFlag(EMode.Edit);
            bool editorRequiresPlay = editorMode.HasFlag(EMode.Play);

            bool editorModeIsTrue = (
                !editorMode.HasFlag(EMode.Edit) || !EditorApplication.isPlaying
            ) && (
                !editorMode.HasFlag(EMode.Play) || EditorApplication.isPlaying
            );

            List<bool> callbackTruly = new List<bool>();
            if(!(editorRequiresEdit && editorRequiresPlay))
            {
                callbackTruly.Add(editorModeIsTrue);
            }

            List<string> errors = new List<string>();

            foreach (string orCallback in targetAttribute.Callbacks)
            {
                (string error, bool isTruly) = Util.GetTruly(target, orCallback);
                if (error != "")
                {
                    errors.Add(error);
                }
                callbackTruly.Add(isTruly);
            }
            foreach ((string callback, Enum enumTarget) in targetAttribute.EnumTargets)
            {
                (string error, Enum result) = Util.GetOf<Enum>(callback, default, property, info, target);
                if (error != "")
                {
                    errors.Add(error);
                    callbackTruly.Add(false);
                }
                else
                {
                    bool isFlag = enumTarget.GetType().GetCustomAttribute<FlagsAttribute>() != null;
                    bool isTruly = isFlag ? result.HasFlag(enumTarget) : result.Equals(enumTarget);
                    callbackTruly.Add(isTruly);
                }
            }

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), false);
            }

            // or, get enabled
            bool truly = callbackTruly.Any(each => each);

            // reverse, get disabled
            return ("", !truly);
        }
    }
}
