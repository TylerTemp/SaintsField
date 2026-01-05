using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class DisableEnableExample : SaintsMonoBehaviour
    {
        public bool boolValue;

        [DisableIf] public int[] justDisable;
        [EnableIf] public int[] justEnable;

        [DisableIf(nameof(boolValue))] public int[] disableIf;
        [EnableIf(nameof(boolValue))] public int[] enableIf;

        [DisableIf("!" + nameof(boolValue))] public int[] NDisableIf;
        [EnableIf("!" + nameof(boolValue))] public int[] NEnableIf;

        [DisableIf(EMode.Edit)] public int[] disableEdit;
        [DisableIf(EMode.Play)] public int[] disablePlay;
        [EnableIf(EMode.Edit)] public int[] enableEdit;
        [EnableIf(EMode.Play)] public int[] enablePlay;

        [Button, DisableIf(nameof(boolValue))] private void DisableIfBtn() => Debug.Log("DisableIfBtn");
        [Button, EnableIf(nameof(boolValue))] private void EnableIfBtn() => Debug.Log("EnableIfBtn");
        [Button, DisableIf(EMode.Edit)] private void DisableEditBtn() => Debug.Log("DisableEditBtn");
        [Button, DisableIf(EMode.Play)] private void DisablePlayBtn() => Debug.Log("DisablePlayBtn");
        [Button, EnableIf(EMode.Edit)] private void EnableEditBtn() => Debug.Log("EnableEditBtn");
        [Button, EnableIf(EMode.Play)] private void EnablePlayBtn() => Debug.Log("EnablePlayBtn");
    }
}
