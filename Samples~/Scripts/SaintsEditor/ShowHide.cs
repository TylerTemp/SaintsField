using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowHide : SaintsMonoBehaviour
    {
        public bool boolValue;

        [HideIf] public int[] justHide;
        [ShowIf] public int[] justShow;

        [HideIf(nameof(boolValue))] public int[] hideIf;
        [ShowIf(nameof(boolValue))] public int[] showIf;

        [HideIf(EMode.Edit)] public int[] hideEdit;
        [HideIf(EMode.Play)] public int[] hidePlay;
        [ShowIf(EMode.Edit)] public int[] showEdit;
        [ShowIf(EMode.Play)] public int[] showPlay;

        [ShowInInspector, HideIf(nameof(boolValue))] public const float HideIfConst = 3.14f;
        [ShowInInspector, ShowIf(nameof(boolValue))] public const float ShowIfConst = 3.14f;
        [ShowInInspector, HideIf(EMode.Edit)] public const float HideEditConst = 3.14f;
        [ShowInInspector, HideIf(EMode.Play)] public const float HidePlayConst = 3.14f;
        [ShowInInspector, ShowIf(EMode.Edit)] public const float ShowEditConst = 3.14f;
        [ShowInInspector, ShowIf(EMode.Play)] public const float ShowPlayConst = 3.14f;

        [ShowInInspector, HideIf(nameof(boolValue))] public static readonly Color HideIfStatic = Color.green;
        [ShowInInspector, ShowIf(nameof(boolValue))] public static readonly Color ShowIfStatic = Color.green;
        [ShowInInspector, HideIf(EMode.Edit)] public static readonly Color HideEditStatic = Color.green;
        [ShowInInspector, HideIf(EMode.Play)] public static readonly Color HidePlayStatic = Color.green;
        [ShowInInspector, ShowIf(EMode.Edit)] public static readonly Color ShowEditStatic = Color.green;
        [ShowInInspector, ShowIf(EMode.Play)] public static readonly Color ShowPlayStatic = Color.green;

        [Button, HideIf(nameof(boolValue))] private void HideIfBtn() => Debug.Log("HideIfBtn");
        [Button, ShowIf(nameof(boolValue))] private void ShowIfBtn() => Debug.Log("ShowIfBtn");
        [Button, HideIf(EMode.Edit)] private void HideEditBtn() => Debug.Log("HideEditBtn");
        [Button, HideIf(EMode.Play)] private void HidePlayBtn() => Debug.Log("HidePlayBtn");
        [Button, ShowIf(EMode.Edit)] private void ShowEditBtn() => Debug.Log("ShowEditBtn");
        [Button, ShowIf(EMode.Play)] private void ShowPlayBtn() => Debug.Log("ShowPlayBtn");
    }
}
