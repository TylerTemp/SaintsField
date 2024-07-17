using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class HideIfExample: MonoBehaviour
    {
        public bool bool1;
        public bool bool2;
        // hide=1||2 => show=!(1||2)=!1&&!2;
        [HideIf(nameof(bool1), nameof(bool2))] public string hide1And2;

        // hide=1||hide=2 => show=!1||show=!2 => show=!1||!2 => show=!(1&&2);
        [HideIf(nameof(bool1)), HideIf(nameof(bool2))] public string hide1Or2;
    }
}
