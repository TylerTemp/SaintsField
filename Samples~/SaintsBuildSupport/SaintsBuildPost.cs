using SaintsBuild;

namespace SaintsField.Samples.SaintsBuildSupport
{
    public class SaintsBuildPost: SaintsMonoBehaviour, IPostProcess
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
