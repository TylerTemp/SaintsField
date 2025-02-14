#if UNITY_2021_3_OR_NEWER
#endif
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute), true)]
    public partial class EnumFlagsAttributeDrawer: SaintsPropertyDrawer
    {
        private Texture2D _checkboxCheckedTexture2D;
        private Texture2D _checkboxEmptyTexture2D;
        private Texture2D _checkboxIndeterminateTexture2D;

        private void LoadIcons()
        {
            _checkboxCheckedTexture2D = Util.LoadResource<Texture2D>("checkbox-checked.png");
            _checkboxEmptyTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-blank.png");
            _checkboxIndeterminateTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-indeterminate.png");
        }

        ~EnumFlagsAttributeDrawer()
        {
            _checkboxCheckedTexture2D = _checkboxEmptyTexture2D = _checkboxIndeterminateTexture2D = null;
        }
    }
}
