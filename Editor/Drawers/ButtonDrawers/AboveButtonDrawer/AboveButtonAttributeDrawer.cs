using System;
using System.Collections.Generic;
using SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ButtonDrawers.AboveButtonDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AboveButtonAttribute), true)]
    public partial class AboveButtonAttributeDrawer: DecButtonAttributeDrawer
    {
        protected override IReadOnlyList<DecButtonShowIfAttribute> GetCurrentShowHide(IReadOnlyList<Attribute> attributes, ISaintsAttribute currentAttribute)
        {
            List<DecButtonShowIfAttribute> showControlAttributes = new List<DecButtonShowIfAttribute>(attributes.Count);

            foreach (Attribute attr in attributes)
            {
                if (ReferenceEquals(attr, currentAttribute))
                {
                    return showControlAttributes;
                }

                if (attr is DecButtonAttribute)
                {
                    showControlAttributes.Clear();
                    continue;
                }

                if (attr is AboveButtonShowIfAttribute showOrHide)  // this includes the HideIf (inherent)
                {
                    showControlAttributes.Add(showOrHide);
                }
            }
            // not found the decorated, skip all
            return Array.Empty<DecButtonShowIfAttribute>();
        }

        protected override IReadOnlyList<DecButtonDisableIfAttribute> GetCurrentDisableEnable(IReadOnlyList<Attribute> attributes, ISaintsAttribute currentAttribute)
        {
            List<DecButtonDisableIfAttribute> disableControlAttributes = new List<DecButtonDisableIfAttribute>(attributes.Count);

            foreach (Attribute attr in attributes)
            {
                if (ReferenceEquals(attr, currentAttribute))
                {
                    return disableControlAttributes;
                }

                if (attr is DecButtonAttribute)
                {
                    disableControlAttributes.Clear();
                    continue;
                }

                if (attr is AboveButtonDisableIfAttribute disableOrEnable)  // this includes the EnableIf (inherent)
                {
                    disableControlAttributes.Add(disableOrEnable);
                }
            }
            // not found the decorated, skip all
            return Array.Empty<DecButtonDisableIfAttribute>();
        }
    }
}
