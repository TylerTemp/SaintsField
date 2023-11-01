using UnityEngine;

namespace ExtInspector.Utils
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

        public static Texture2D ConvertToCompatibleFormat(Texture2D texture)
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
            Debug.Log($"ResizeTexture resize {newWidth}x{newHeight}");
            TextureScale.Scale(originalTexture, newWidth, newHeight);
        }

        // return resizedTexture;
        public static void ResizeHeightTexture(Texture2D originalTexture, int newHeight)
        {
            int oriWidth = originalTexture.width;
            int oriHeight = originalTexture.height;
            int newWidth = Mathf.RoundToInt((float)oriWidth * newHeight / oriHeight);
            // Debug.Log($"resize texture from {oriWidth}x{oriHeight} to {newWidth}x{newHeight}");
            TextureScale.Scale(originalTexture, newWidth, newHeight);
        }

        public static void ResizeWidthTexture(Texture2D originalTexture, int newWidth)
        {
            int oriWidth = originalTexture.width;
            int oriHeight = originalTexture.height;
            int newHeight = Mathf.RoundToInt((float)oriHeight * newWidth / oriWidth);
            // Debug.Log($"resize texture from {oriWidth}x{oriHeight} to {newWidth}x{newHeight}");
            TextureScale.Scale(originalTexture, newWidth, newHeight);
        }

        public static Texture2D TextureTo(Texture2D texture, Color color, int width=-1, int height=-1) {
            Texture2D colored = ApplyTextureColor(texture, color);
            if (height == -1 && width == -1)
            {
            }
            else if (width != -1 && height != -1)  // 缩放
            {
                int maxWidth = width;
                int maxHeight = height;
                int oriWidth = texture.width;
                int oriHeight = texture.height;
                float aspectRatio = (float)oriWidth / oriHeight;

                if (oriWidth > maxWidth || oriHeight > maxHeight)
                {
                    if (oriWidth / (float)maxWidth > oriHeight / (float)maxHeight)
                    {
                        oriWidth = maxWidth;
                        oriHeight = Mathf.RoundToInt(maxWidth / aspectRatio);
                    }
                    else
                    {
                        oriHeight = maxHeight;
                        oriWidth = Mathf.RoundToInt(maxHeight * aspectRatio);
                    }
                }
                ResizeTexture(colored, oriWidth, oriHeight);
            }
            else if(width == -1)
            {
                ResizeHeightTexture(colored, height);
            }
            else
            {
                ResizeTexture(colored, width, height);
            }
            return colored;
        }

        public static Texture2D TextureTo(Texture2D texture, int width=-1, int height=-1) {
            Texture2D colored = ConvertToCompatibleFormat(texture);
            if (height == -1 && width == -1)  // 不变
            {
                return colored;
            }

            if (width != -1 && height != -1)  // 缩放
            {
                int maxWidth = width;
                int maxHeight = height;
                int oriWidth = texture.width;
                int oriHeight = texture.height;
                float aspectRatio = (float)oriWidth / oriHeight;

                if (oriWidth > maxWidth || oriHeight > maxHeight)
                {
                    if (oriWidth / (float)maxWidth > oriHeight / (float)maxHeight)
                    {
                        oriWidth = maxWidth;
                        oriHeight = Mathf.RoundToInt(maxWidth / aspectRatio);
                    }
                    else
                    {
                        oriHeight = maxHeight;
                        oriWidth = Mathf.RoundToInt(maxHeight * aspectRatio);
                    }
                }
                ResizeTexture(colored, oriWidth, oriHeight);
            }
            else if(width == -1)  // 高度适配
            {
                ResizeHeightTexture(colored, height);
            }
            else if (height == -1) // 宽度适配
            {
                ResizeWidthTexture(colored, width);
            }
            else
            {
                // Debug.Log($"TextureTo resize {width}x{height}");
                ResizeTexture(colored, width, height);
            }
            return colored;
        }
    }
}
