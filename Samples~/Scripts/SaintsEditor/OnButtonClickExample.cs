using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class OnButtonClickExample: SaintsMonoBehaviour
    {
        [OnButtonClick]
        public void OnButtonClickVoid()
        {
            Debug.Log("OnButtonClick Void");
        }

        [OnButtonClick(value: 2)]
        public void OnButtonClickInt(int value)
        {
            Debug.Log($"OnButtonClick ${value}");
        }

        [OnButtonClick(value: true)]
        public void OnButtonClickBool(bool value)
        {
            Debug.Log($"OnButtonClick ${value}");
        }

        [OnButtonClick(value: 0.3f)]
        public void OnButtonClickFloat(float value)
        {
            Debug.Log($"OnButtonClick ${value}");
        }

        private GameObject ThisGo => this.gameObject;

        [OnButtonClick(value: nameof(ThisGo), isCallback: true)]
        public void OnButtonClickComp(UnityEngine.Object value)
        {
            Debug.Log($"OnButtonClick ${value}");
        }
    }
}
