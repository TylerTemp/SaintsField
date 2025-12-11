using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorResizableTextArea: SaintsMonoBehaviour
    {
        [ResizableTextArea]
        public string ta;

        [ShowInInspector, ResizableTextArea]
        public string RawTa
        {
            get => ta;
            set => ta = value;
        }
    }
}
