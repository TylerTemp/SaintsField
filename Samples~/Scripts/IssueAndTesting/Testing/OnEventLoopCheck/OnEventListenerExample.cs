using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing.OnEventLoopCheck
{
    public class OnEventListenerExample : SaintsMonoBehaviour
    {
        [GetComponent] public EventProviderExample example;

        [OnEvent(nameof(example) + ".evt")]
        public void OnEvent()
        {
            Debug.Log("OnEvent");
        }
    }
}
