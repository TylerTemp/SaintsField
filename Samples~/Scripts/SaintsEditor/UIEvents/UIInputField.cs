using TMPro;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.UIEvents
{
    public class UIInputField : SaintsMonoBehaviour
    {
        [OnInputFieldEndEdit]  // bind a void
        public void InputFieldEndEdit()
        {
            Debug.Log("InputFieldEndEdit Void");
        }

        [OnInputFieldChanged]  // bind to receive the value
        public void OnInputFieldChanged(string value)
        {
            Debug.Log($"InputFieldChanged {value}");
        }

        [OnInputFieldSelect(value: "My Custom String")]  // bind with a static value callback
        public void OtherMethodStr(string str)
        {
            Debug.Log($"OtherMethod {str}");
        }

        [GetInSiblings] public TMP_InputField inputField;

        public TMP_InputField GetMyInputField() => inputField;

        [OnInputFieldDeselect(nameof(GetMyInputField))] // taget is a callback (support field/property/function)
        public void BindFromOtherTarget(string val)
        {
            Debug.Log($"BindFromOtherTarget {val}");
        }
    }
}
