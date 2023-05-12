using UnityEngine;

namespace ExtInspector.Standalone
{
    public static class Icon
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

        public static void ResizeTexture(Texture2D originalTexture, int newWidth, int newHeight)
        {
            // Create a new texture with the desired width and height
            // Texture2D resizedTexture = new Texture2D(newWidth, newHeight);

            // Resize the texture using the TextureScale.Bilinear method
            // TextureScale.Bilinear(originalTexture, resizedTexture);
            TextureScale.Scale(originalTexture, newWidth, newHeight);

            // return resizedTexture;
        }
    }
}
