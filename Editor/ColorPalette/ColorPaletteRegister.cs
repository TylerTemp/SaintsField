using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.ColorPalette
{
    public class ColorPaletteRegister: AssetPostprocessor
    {
        public static readonly List<SaintsField.ColorPalette> ColorPalettes = new List<SaintsField.ColorPalette>();
        public static readonly UnityEvent OnColorPalettesChanged = new UnityEvent();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            ColorPalettes.Clear();
            RegisterColorPalettes();
        }

        [InitializeOnLoadMethod]
        private static void RegisterColorPalettes()
        {
            if (ColorPalettes.Count != 0)
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:" + nameof(SaintsField.ColorPalette));
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SaintsField.ColorPalette colorPalette = AssetDatabase.LoadAssetAtPath<SaintsField.ColorPalette>(path);
                if(colorPalette != null)
                {
                    ColorPalettes.Add(colorPalette);
                }
            }
#if SAINTSFIELD_DEBUG
            Debug.Log($"Found color palettes: {string.Join(", ", ColorPalettes.Select(each => $"{each.displayName}:{each.colors.Count}"))}");
#endif

            OnColorPalettesChanged.Invoke();
        }
    }
}
