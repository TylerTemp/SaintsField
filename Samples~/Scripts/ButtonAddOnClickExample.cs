using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples.Scripts
{
    public class ButtonAddOnClickExample: MonoBehaviour
    {
        [GetComponent, ButtonAddOnClick(nameof(OnClick))] public Button button;

        private void OnClick()
        {
            Debug.Log("Button clicked!");
        }
    }
}
