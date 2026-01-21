using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ScriptableRenderer
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(
        fileName = "ScriptableRendererDataTarget",
        menuName = "SAINTSFIELD_DEBUG/ScriptableRendererDataTarget")]
#endif
    public class ScriptableRendererDataTarget: SaintsField.ScriptableRenderer.SaintsScriptableRendererData
    {
        protected override UnityEngine.Rendering.Universal.ScriptableRenderer Create()
        {
            return new ScriptableRendererExample(this);
        }
    }
}
