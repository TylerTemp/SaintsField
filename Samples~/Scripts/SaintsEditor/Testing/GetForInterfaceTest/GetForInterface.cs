using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.GetForInterfaceTest
{
    public partial class GetForInterface : SaintsMonoBehaviour
    {
        [SaintsSerialized, GetInChildren] private IMyInterface[] _myInterfaces;
    }
}
