#if UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer
{
    public static class ShaderKeywordUtils
    {
        public static IEnumerable<string> GetShaderKeywords(Shader shader)
        {
            LocalKeywordSpace keywordSpace = shader.keywordSpace;

            foreach (LocalKeyword localKeyword in keywordSpace.keywords)
            {
                yield return localKeyword.name;
            }
        }
    }
}
#endif
