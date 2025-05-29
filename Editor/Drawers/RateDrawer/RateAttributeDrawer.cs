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
        private Texture2D _star;
        private Texture2D _starSlash;

        private Texture2D _starSlashActive;
        private Texture2D _starSlashInactive;
        private Texture2D _starActive;
        private Texture2D _starIncrease;
        private Texture2D _starDecrease;
        private Texture2D _starInactive;

        private GUIContent _guiContentSlash;
        private GUIContent _guiContentSlashInactive;
        private GUIContent _guiContentActive;
        private GUIContent _guiContentIncrease;
        private GUIContent _guiContentDecrease;
        private GUIContent _guiContentInactive;

        // private Texture2D _clear;

        private GUIStyle _normalClear;
        private GUIStyle _normalFramed;

        private static readonly Color ActiveColor = Color.yellow;
        private static readonly Color WillActiveColor = new Color(228/255f, 1, 0, 0.7f);
        private static readonly Color WillInactiveColor = new Color(100/255f, 100/255f, 0, 1f);
        private static readonly Color InactiveColor = Color.grey;

        // private static Texture2D MakePixel(Color color)
        // {
        //     Color[] pix = { color };
        //     Texture2D result = new Texture2D(1, 1);
        //     result.SetPixels(pix);
        //     result.Apply();
        //     return result;
        // }

#if UNITY_2021_3_OR_NEWER

#endif
    }
}
