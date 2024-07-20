using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowHide : SaintsMonoBehaviour
    {
        public bool boolValue;

        [PlayaHideIf] public int[] justHide;
        [PlayaShowIf] public int[] justShow;

        [PlayaHideIf(nameof(boolValue))] public int[] hideIf;
        [PlayaShowIf(nameof(boolValue))] public int[] showIf;

        [PlayaHideIf(EMode.Edit)] public int[] hideEdit;
        [PlayaHideIf(EMode.Play)] public int[] hidePlay;
        [PlayaShowIf(EMode.Edit)] public int[] showEdit;
        [PlayaShowIf(EMode.Play)] public int[] showPlay;

        [ShowInInspector, PlayaHideIf(nameof(boolValue))] public const float HideIfConst = 3.14f;
        [ShowInInspector, PlayaShowIf(nameof(boolValue))] public const float ShowIfConst = 3.14f;
        [ShowInInspector, PlayaHideIf(EMode.Edit)] public const float HideEditConst = 3.14f;
        [ShowInInspector, PlayaHideIf(EMode.Play)] public const float HidePlayConst = 3.14f;
        [ShowInInspector, PlayaShowIf(EMode.Edit)] public const float ShowEditConst = 3.14f;
        [ShowInInspector, PlayaShowIf(EMode.Play)] public const float ShowPlayConst = 3.14f;

        [ShowInInspector, PlayaHideIf(nameof(boolValue))] public static readonly Color HideIfStatic = Color.green;
        [ShowInInspector, PlayaShowIf(nameof(boolValue))] public static readonly Color ShowIfStatic = Color.green;
        [ShowInInspector, PlayaHideIf(EMode.Edit)] public static readonly Color HideEditStatic = Color.green;
        [ShowInInspector, PlayaHideIf(EMode.Play)] public static readonly Color HidePlayStatic = Color.green;
        [ShowInInspector, PlayaShowIf(EMode.Edit)] public static readonly Color ShowEditStatic = Color.green;
        [ShowInInspector, PlayaShowIf(EMode.Play)] public static readonly Color ShowPlayStatic = Color.green;

        [Button, PlayaHideIf(nameof(boolValue))] private void HideIfBtn() => Debug.Log("HideIfBtn");
        [Button, PlayaShowIf(nameof(boolValue))] private void ShowIfBtn() => Debug.Log("ShowIfBtn");
        [Button, PlayaHideIf(EMode.Edit)] private void HideEditBtn() => Debug.Log("HideEditBtn");
        [Button, PlayaHideIf(EMode.Play)] private void HidePlayBtn() => Debug.Log("HidePlayBtn");
        [Button, PlayaShowIf(EMode.Edit)] private void ShowEditBtn() => Debug.Log("ShowEditBtn");
        [Button, PlayaShowIf(EMode.Play)] private void ShowPlayBtn() => Debug.Log("ShowPlayBtn");
    }
}
