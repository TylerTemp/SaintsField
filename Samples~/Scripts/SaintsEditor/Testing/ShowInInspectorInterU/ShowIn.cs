using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.ShowInInspectorInterU
{
    public class ShowIn : SaintsMonoBehaviour
    {
        [ShowInInspector] private IInterface _myMonoInter;
        [ShowInInspector] private INoImplement _noImplement;
        [ShowInInspector] private IMixInterface _mix;
    }
}
