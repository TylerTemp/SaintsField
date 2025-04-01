using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.FullWidthRichLabelDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AboveRichLabelAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowRichLabelAttribute), true)]
    [CustomPropertyDrawer(typeof(FullWidthRichLabelAttribute), true)]
    public partial class FullWidthRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        // public bool IsSaintsPropertyDrawerOverrideLabel;
    }
}
