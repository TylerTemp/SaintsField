using SaintsBuild;
using UnityEngine;

namespace SaintsField.Samples.SaintsBuildSupport
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu]
#endif
    public class SaintsBuildPostSo: SaintsScriptableObject, IPostProcess
    {
        public string strField;

#if UNITY_EDITOR
        public bool EditorOnPostProcess(PostProcessInfo postProcessInfo)
        {
            return true;
        }
#endif
    }
}
