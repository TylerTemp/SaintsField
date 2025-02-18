using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawWireDiscDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(DrawWireDiscAttributeDrawer), true)]
    public partial class DrawWireDiscAttributeDrawer: SaintsPropertyDrawer
    {
        private class WireDiscInfo
        {
            public float Radius;
            public EColor EColor;
            public Util.TargetWorldPosInfo TargetWorldPosInfo;
        }
    }
}
