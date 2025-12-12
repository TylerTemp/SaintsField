using SaintsField;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorPropRange : SaintsMonoBehaviour
    {
        public int min;
        public int max;

        [PropRange(nameof(min), nameof(max))] public int propRange;

        [ShowInInspector, PropRange(nameof(min), nameof(max))]
        public int RawPropRange
        {
            get => propRange;
            set => propRange = value;
        }
    }
}
