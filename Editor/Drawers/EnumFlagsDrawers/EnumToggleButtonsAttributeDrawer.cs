#if UNITY_2021_3_OR_NEWER
#endif
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(EnumToggleButtonsAttribute), true)]
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute), true)]
    public partial class EnumToggleButtonsAttributeDrawer: SaintsPropertyDrawer
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

        private static IEnumerable<KeyValuePair<int, EnumFlagsUtil.EnumDisplayInfo>> GetDisplayBit(EnumFlagsMetaInfo metaInfo)
        {
            if (metaInfo.HasFlags)
            {
                return metaInfo.BitValueToName
                    .Where(each => each.Key != 0 && each.Key != metaInfo.AllCheckedInt);

            }
            return metaInfo.BitValueToName;
        }
    }
}
