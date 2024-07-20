using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class DisableEnableExample : SaintsMonoBehaviour
    {
        public bool boolValue;

        [PlayaDisableIf] public int[] justDisable;
        [PlayaEnableIf] public int[] justEnable;

        [PlayaDisableIf(nameof(boolValue))] public int[] disableIf;
        [PlayaEnableIf(nameof(boolValue))] public int[] enableIf;

        [PlayaDisableIf("!" + nameof(boolValue))] public int[] NDisableIf;
        [PlayaEnableIf("!" + nameof(boolValue))] public int[] NEnableIf;

        [PlayaDisableIf(EMode.Edit)] public int[] disableEdit;
        [PlayaDisableIf(EMode.Play)] public int[] disablePlay;
        [PlayaEnableIf(EMode.Edit)] public int[] enableEdit;
        [PlayaEnableIf(EMode.Play)] public int[] enablePlay;

        [Button, PlayaDisableIf(nameof(boolValue))] private void DisableIfBtn() => Debug.Log("DisableIfBtn");
        [Button, PlayaEnableIf(nameof(boolValue))] private void EnableIfBtn() => Debug.Log("EnableIfBtn");
        [Button, PlayaDisableIf(EMode.Edit)] private void DisableEditBtn() => Debug.Log("DisableEditBtn");
        [Button, PlayaDisableIf(EMode.Play)] private void DisablePlayBtn() => Debug.Log("DisablePlayBtn");
        [Button, PlayaEnableIf(EMode.Edit)] private void EnableEditBtn() => Debug.Log("EnableEditBtn");
        [Button, PlayaEnableIf(EMode.Play)] private void EnablePlayBtn() => Debug.Log("EnablePlayBtn");
    }
}
