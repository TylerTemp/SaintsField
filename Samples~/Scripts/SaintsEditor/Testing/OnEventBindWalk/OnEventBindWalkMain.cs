using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.OnEventBindWalk
{
    public class OnEventBindWalkMain : SaintsMonoBehaviour
    {
        [SerializeField, AddComponent, GetComponent]
        private OnEventTarget _i2Callback;

        [OnEvent(nameof(_i2Callback) + "." + nameof(OnEventTarget._OnLocalize))]
        public void OnLocalize()
        {
        }
    }
}
