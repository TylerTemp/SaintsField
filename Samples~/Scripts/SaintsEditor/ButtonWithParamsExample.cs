using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ButtonWithParamsExample : MonoBehaviour
    {
        [Button]
        private void OnButton()
        {
            Debug.Log("Button clicked");
        }

        [Button]
        private void OnButtonParams(UnityEngine.Object myObj, int myInt, string myStr = "hi")
        {
            Debug.Log($"{myObj}, {myInt}, {myStr}");
        }
    }
}
