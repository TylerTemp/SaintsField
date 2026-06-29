using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples.Scripts.SaintsEditor.UIEvents
{
    public class UIToggle : SaintsMonoBehaviour
    {
        [OnToggleChanged]  // bind a void
        public void ToggleChangedVoid()
        {
            Debug.Log("ToggleChanged Void");
        }

        [OnToggleChanged]  // bind to receive the value
        public void ToggleChangedBool(bool isOn)
        {
            Debug.Log($"ToggleChanged {isOn}");
        }

        [OnToggleChanged(value: "My Custom String")]  // bind with a static value callback
        public void OtherMethodStr(string str)
        {
            Debug.Log($"OtherMethod {str}");
        }

        [GetInSiblings] public Toggle toggle;

        public Toggle GetMyToggle() => toggle;

        [OnToggleChanged(nameof(GetMyToggle))] // taget is a callback (support field/property/function)
        public void BindFromOtherTarget(bool isOn)
        {
            Debug.Log($"BindFromOtherTarget {isOn}");
        }
    }
}
