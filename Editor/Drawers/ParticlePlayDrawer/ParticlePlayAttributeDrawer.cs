using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ParticlePlayDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ParticlePlayAttribute), true)]
    public partial class ParticlePlayAttributeDrawer: SaintsPropertyDrawer
    {
        private enum PlayState
        {
            None,  // not playing at all
            Playing,
            Paused,
        }

    }
}
