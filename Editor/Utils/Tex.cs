using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class Tex
    {
        public static (int width, int height) GetProperScaleRect(int maxWidth, int widthLimit, int heightLimit, int curWidth, int curHeight)
        {
            Debug.Assert(maxWidth > 0, $"maxWidth {maxWidth} <= 0");

            (int rectScaleWidth, int rectScaleHeight) = FitScale(widthLimit, heightLimit, curWidth, curHeight);
            // Debug.Log($"first fit {rectScaleWidth}x{rectScaleHeight} from {curWidth}x{curHeight} by {widthLimit}x{heightLimit}");
            if(rectScaleWidth > maxWidth)
            {
                (rectScaleWidth, rectScaleHeight) = FitScale(maxWidth, heightLimit, rectScaleWidth, rectScaleHeight);
            }

            return (rectScaleWidth, rectScaleHeight);
        }

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

        // ReSharper disable once MemberCanBePrivate.Global
        public static Texture2D ConvertToCompatibleFormat(Texture2D texture)
        {
            // ReSharper disable once ConvertToUsingDeclaration
            using (RenderTextureTemporaryScoop renderTextureTemporary = new RenderTextureTemporaryScoop(
                       texture.width,
                       texture.height,
                       0,
                       RenderTextureFormat.Default,
                       RenderTextureReadWrite.Linear))
            {
                RenderTexture renderTex = renderTextureTemporary.RenderTex;

                Graphics.Blit(texture, renderTex);


                // Create a new texture with a compatible format
                Texture2D convertedTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

                using(new RenderTextureActiveScoop(renderTex))
                {
                    convertedTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                }
                convertedTexture.Apply();

                return convertedTexture;
            }
        }

        public static void ResizeTexture(Texture2D originalTexture, int newWidth, int newHeight)
        {
            // Debug.Log($"ResizeTexture resize {newWidth}x{newHeight}");
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

        public static (int width, int height) FitScale(int widthLimit, int heightLimit, int oriWidth, int oriHeight)
        {
            if (widthLimit != -1 && heightLimit != -1)  // scale to rect
            {
                float aspectRatio = (float)oriWidth / oriHeight;

                int newWidth;
                int newHeight;

                // ReSharper disable once InvertIf
                // if (oriWidth > widthLimit || oriHeight > heightLimit)
                // {
                //
                // }
                if (oriWidth / (float)widthLimit > oriHeight / (float)heightLimit)
                {
                    newWidth = widthLimit;
                    newHeight = Mathf.RoundToInt(widthLimit / aspectRatio);
                }
                else
                {
                    newHeight = heightLimit;
                    newWidth = Mathf.RoundToInt(heightLimit * aspectRatio);
                }

                return (newWidth, newHeight);
            }

            if(widthLimit == -1 && heightLimit == -1)
            {
                return (oriWidth, oriHeight);
            }

            if(widthLimit == -1)
            {
                int newWidth = Mathf.RoundToInt((float)oriWidth * heightLimit / oriHeight);
                // Debug.Log($"newWidth {newWidth} = oriWidth {oriWidth} * heightLimit {heightLimit} / oriHeight {oriHeight}");
                return (newWidth, heightLimit);
            }
            // ReSharper disable once InvertIf
            if (heightLimit == -1)
            {
                int newHeight = Mathf.RoundToInt((float)oriHeight * widthLimit / oriWidth);
                // Debug.Log($"newHeight {newHeight} = oriHeight {oriHeight} * widthLimit {widthLimit} / oriWidth {oriWidth}");
                return (widthLimit, newHeight);
            }
            // -1, -1
            // Debug.Log($"return ori {oriWidth}x{oriHeight}");
            // return (oriWidth, oriHeight);
            throw new System.Exception($"FitScale {widthLimit}x{heightLimit} {oriWidth}x{oriHeight}");
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
