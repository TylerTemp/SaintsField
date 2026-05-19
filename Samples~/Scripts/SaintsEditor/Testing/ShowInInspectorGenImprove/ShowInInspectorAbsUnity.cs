using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.ShowInInspectorGenImprove
{
    public class ShowInInspectorAbsUnity : SaintsMonoBehaviour
    {
        [ShowInInspector] private Object _unityObject;
        [ShowInInspector] private AbsUnityObject _absObject;
    }
}
