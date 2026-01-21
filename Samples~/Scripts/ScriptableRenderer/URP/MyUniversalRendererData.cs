#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

using SaintsField.ScriptableRenderer;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SaintsField.Samples.Scripts.ScriptableRenderer.URP
{
    public class MyUniversalRendererData : SaintsUniversalRendererData
    {
        [SerializeField] public AnimationCurve animationCurve;

#if UNITY_EDITOR
        internal class CreateSaintsUniversalRendererAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                MyUniversalRendererData rendererData = CreateInstance<MyUniversalRendererData>();
                // rendererData.postProcessData = PostProcessData.GetDefaultPostProcessData();
                AssetDatabase.CreateAsset(rendererData, pathName);
                ResourceReloader.ReloadAllNullIn(rendererData, UniversalRenderPipelineAsset.packagePath);
                Selection.activeObject = rendererData;
            }
        }

#if SAINTSFIELD_DEBUG
        [MenuItem("Assets/Create/Rendering/My URP Universal Renderer", priority = CoreUtils.Sections.section3 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority + 2)]
#endif
        private static void CreateUniversalRendererData()
        {
            Texture2D icon = CoreUtils.GetIconForType<ScriptableRendererData>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateSaintsUniversalRendererAsset >(), "New Custom Universal Renderer Data.asset", icon, null);
        }
#endif
    }
}
