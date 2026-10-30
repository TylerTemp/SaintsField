using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class ButtonSer : SaintsMonoBehaviour
    {
        private struct St: IInterface2
        {
            public int IntV;
            public Transform Comp;
        }

        [Button]
        private void BtnParams(int number, GameObject uG, IInterface2 interf, St st)
        {

        }
    }
}
