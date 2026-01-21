using SaintsField.ScriptableRenderer.Urp;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ScriptableRenderer.URP
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(
        fileName = "ScriptableRendererDataTarget",
        menuName = "SAINTSFIELD_DEBUG/ScriptableRendererDataTarget")]
#endif
    public class ScriptableRendererDataTarget: SaintsScriptableRendererData
    {
        protected override UnityEngine.Rendering.Universal.ScriptableRenderer Create()
        {
            return new ScriptableRendererExample(this);
        }
    }
}
