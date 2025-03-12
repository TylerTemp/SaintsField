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
            if (CheckColorPalettes())
            {
                OnColorPalettesChanged.Invoke();
            }
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethodCheck()
        {
            if (ColorPalettes.Count != 0)
            {
                return;
            }

            if(CheckColorPalettes())
            {
                OnColorPalettesChanged.Invoke();
            }
        }

        private static bool CheckColorPalettes()
        {
            List<SaintsField.ColorPalette> nowColorPalettes = new List<SaintsField.ColorPalette>();
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(SaintsField.ColorPalette));
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SaintsField.ColorPalette colorPalette = AssetDatabase.LoadAssetAtPath<SaintsField.ColorPalette>(path);
                if(colorPalette != null)
                {
                    nowColorPalettes.Add(colorPalette);
                }
            }

            bool changed = false;

            foreach (SaintsField.ColorPalette nowColorPalette in nowColorPalettes
                         .Where(nowColorPalette => !ColorPalettes.Contains(nowColorPalette)))
            {
                changed = true;
                ColorPalettes.Add(nowColorPalette);
#if SAINTSFIELD_DEBUG
                Debug.Log($"Add color palettes: {nowColorPalette.displayName}:{ColorPalettes.Count}");
#endif
            }

            foreach (SaintsField.ColorPalette oldColorPalette in ColorPalettes.ToArray())
            {
                // ReSharper disable once InvertIf
                if(!nowColorPalettes.Contains(oldColorPalette))
                {
                    changed = true;
                    ColorPalettes.Remove(oldColorPalette);
#if SAINTSFIELD_DEBUG
                    Debug.Log($"Remove color palettes: {oldColorPalette.displayName}:{ColorPalettes.Count}");
#endif
                }
            }

            return changed;
        }
    }
}
