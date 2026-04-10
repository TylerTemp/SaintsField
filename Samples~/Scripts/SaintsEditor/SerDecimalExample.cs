using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SerDecimalExample : SaintsMonoBehaviour
    {
        [SaintsSerialized, OnValueChanged(":Debug.Log")] private decimal _d;

        [ShowInInspector]
        private decimal D
        {
            get => _d;
            set => _d = value;
        }
    }
}
