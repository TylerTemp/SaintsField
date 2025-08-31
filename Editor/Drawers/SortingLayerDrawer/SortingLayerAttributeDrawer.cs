using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SortingLayerAttribute), true)]
    public partial class SortingLayerAttributeDrawer : SaintsPropertyDrawer
    {
    }
}
