using System;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class RenderTextureTemporaryScoop: IDisposable
    {
        public readonly RenderTexture RenderTex;

        public RenderTextureTemporaryScoop(int width,
            int height,
            int depthBuffer,
            RenderTextureFormat format,
            RenderTextureReadWrite readWrite)
        {
            RenderTex = RenderTexture.GetTemporary(
                width,
                height,
                depthBuffer,
                format,
                readWrite);
        }

        public void Dispose()
        {
            RenderTexture.ReleaseTemporary(RenderTex);
        }
    }
}
