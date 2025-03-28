using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class OverrideButton : OverloadButton
    {
        [Button]
        protected override void Override(char c) => Debug.Log($"Override char {c}");
    }
}
