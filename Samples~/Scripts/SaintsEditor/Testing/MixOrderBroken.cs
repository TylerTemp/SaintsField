using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class MixOrderBroken : SaintsMonoBehaviour
    {
        [LayoutStart("S", ELayout.FoldoutBox)]
        [SerializeField] private GameObject _go;
        [LayoutEnd]
        private void FuncHere(){}

        [ShowInInspector] private const int ConstInt = 5;

        [LayoutStart("events", ELayout.FoldoutBox)]
        public GameObject _e1;

        [LayoutEnd]
        private event System.Action _eventKeywordTarget;

        [ShowInInspector] private const int Const2 = 5;
    }
}
