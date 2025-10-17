using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    // Note the `partial`!
    public partial class SerInterfaceExample : SaintsMonoBehaviour
    {
        [SaintsSerialized] private IInterface1 _interface1;
    }
}
