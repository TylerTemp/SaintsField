using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.RateDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(RateAttribute), true)]
    public partial class RateAttributeDrawer: SaintsPropertyDrawer
    {
        private static Texture2D _star;
        private static Texture2D _starSlash;

        private static GUIStyle _normalClear;
        private static GUIStyle _normalFramed;
    }
}
