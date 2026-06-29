using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples.Scripts.SaintsEditor.UIEvents
{
    public class UISlider : SaintsMonoBehaviour
    {
        [OnSliderChanged]  // bind a void
        public void SliderChangedVoid()
        {
            Debug.Log("SliderChanged Void");
        }

        [OnSliderChanged]  // bind to receive the value
        public void SliderChangedFloat(float value)
        {
            Debug.Log($"SliderChanged {value}");
        }

        [OnSliderChanged(value: "My Custom String")]  // bind with a static value callback
        public void OtherMethodStr(string str)
        {
            Debug.Log($"OtherMethod {str}");
        }

        [GetInSiblings] public Slider slider;

        public Slider GetMySlider() => slider;

        [OnSliderChanged(nameof(GetMySlider))] // taget is a callback (support field/property/function)
        public void BindFromOtherTarget(float s)
        {
            Debug.Log($"BindFromOtherTarget {s}");
        }
    }
}
