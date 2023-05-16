using UnityEngine;

namespace ExtInspector.Standalone
{
    public static class Tex
    {
        public static Texture2D ApplyTextureColor(Texture2D texture, Color newColor)
        {
            Texture2D convertedTexture = ConvertToCompatibleFormat(texture);

            // Modify the color of the converted texture
            Color[] pixels = convertedTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] *= newColor;
            }
            convertedTexture.SetPixels(pixels);
            convertedTexture.Apply();

            return convertedTexture;
        }

        private static Texture2D ConvertToCompatibleFormat(Texture2D texture)
        {
            // Create a new texture with a compatible format
            Texture2D convertedTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

            // Copy the pixel data from the original texture to the converted texture
            Color[] pixels = texture.GetPixels();
            convertedTexture.SetPixels(pixels);
            convertedTexture.Apply();

            return convertedTexture;
        }

        public static void ResizeTexture(Texture2D originalTexture, int newWidth, int newHeight) =>
            TextureScale.Scale(originalTexture, newWidth, newHeight);

        // return resizedTexture;
        public static void ResizeHeightTexture(Texture2D originalTexture, int newHeight)
        {
            int oriWidth = originalTexture.width;
            int oriHeight = originalTexture.height;
            int newWidth = Mathf.RoundToInt((float)oriWidth * newHeight / oriHeight);
            TextureScale.Scale(originalTexture, newWidth, newHeight);
        }

        public static Texture2D TextureTo(Texture2D texture, Color color, int height) {
            Texture2D colored = ApplyTextureColor(texture, color);
            ResizeHeightTexture(colored, height);
            return colored;
        }
    }
}
