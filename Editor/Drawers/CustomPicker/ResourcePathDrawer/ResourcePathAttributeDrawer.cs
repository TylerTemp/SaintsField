using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.CustomPicker.RequireTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.ResourcePathDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ResourcePathAttribute), true)]
    public partial class ResourcePathAttributeDrawer : RequireTypeAttributeDrawer
    {
    }
}
