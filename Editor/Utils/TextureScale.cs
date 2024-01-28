// Modified from: http://wiki.unity3d.com/index.php/TextureScale#TextureScale.cs
// Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable

using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class TextureScale
    {
        public static void Scale(Texture2D tex, int newWidth, int newHeight) {
            Color[] texColors = tex.GetPixels();
            Color[] newColors = new Color[newWidth * newHeight];
            float ratioX = 1.0f / ((float)newWidth / (tex.width-1));
            float ratioY = 1.0f / ((float)newHeight / (tex.height-1));
            int w = tex.width;
            int w2 = newWidth;

            BilinearScale(0, newHeight, ratioY, w, w2, ratioX, texColors, newColors);

            // Texture2D result = new Texture2D(newWidth, newHeight);

            // Debug.Log($"resize texture from {tex.width}x{tex.height} to {newWidth}x{newHeight}");
#if UNITY_2021_2_OR_NEWER
            tex.Reinitialize(newWidth, newHeight);
#else
            tex.Resize(newWidth, newHeight);
#endif
            tex.SetPixels(newColors);
            tex.Apply();
            // return result;
        }

        private static void BilinearScale(int start, int end, float ratioY, int w, int w2, float ratioX, IReadOnlyList<Color> texColors, IList<Color> newColors) {
            for (int y = start; y < end; y++) {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                int y1 = yFloor * w;
                int y2 = (yFloor+1) * w;
                int yw = y * w2;

                for (int x = 0; x < w2; x++) {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    float xLerp = x * ratioX-xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor+1], xLerp),
                        ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor+1], xLerp),
                        y*ratioY-yFloor);
                }
            }
        }

        private static Color ColorLerpUnclamped (Color c1, Color c2, float value) {
            return new Color (c1.r + (c2.r - c1.r) * value,
                c1.g + (c2.g - c1.g) * value,
                c1.b + (c2.b - c1.b) * value,
                c1.a + (c2.a - c1.a) * value);
        }
    }
}
