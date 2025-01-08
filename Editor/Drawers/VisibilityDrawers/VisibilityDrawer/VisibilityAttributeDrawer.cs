using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.VisibilityDrawers.VisibilityDrawer
{
    public abstract partial class VisibilityAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerSkipDrawer
    {
        public bool AutoRunnerSkip(SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            (string error, bool show) = GetShow(property, memberInfo);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (error != "")
            {
                return true;
            }

            return !show;
        }

        private static (string error, bool show) GetShow(SerializedProperty property, MemberInfo info)
        {
            (ShowIfAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ShowIfAttribute>(property);

            List<bool> showOrResults = new List<bool>();
            string error = "";
            foreach (ShowIfAttribute showIfAttribute in attributes)
            {
                (string error, bool shown) showResult = showIfAttribute.IsShow
                    ? ShowIfAttributeDrawer.HelperShowIfIsShown(showIfAttribute.ConditionInfos, property, info, parent)
                    : HideIfAttributeDrawer.HelperHideIfIsShown(showIfAttribute.ConditionInfos, property, info, parent);

                if (showResult.error != "")
                {
                    error = showResult.error;
                    break;
                }
                showOrResults.Add(showResult.shown);
            }

            if (error != "")
            {
                return (error, true);
            }

            Debug.Assert(showOrResults.Count > 0);
            return ("", showOrResults.Any(each => each));
        }
    }
}
