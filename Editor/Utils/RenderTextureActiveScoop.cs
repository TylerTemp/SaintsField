using System;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public readonly struct RenderTextureActiveScoop: IDisposable
    {
        private readonly RenderTexture _previousRenderTexture;

        public RenderTextureActiveScoop(RenderTexture nowActive)
        {
            _previousRenderTexture = RenderTexture.active;
            RenderTexture.active = nowActive;
        }

        public void Dispose()
        {
            RenderTexture.active = _previousRenderTexture;
        }
    }
}
