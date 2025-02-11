using SaintsField.Spine;
using Spine.Unity;
using UnityEngine;

namespace SaintsField.Samples.Spine.Scripts
{
    public class SpineSkinExample : MonoBehaviour
    {
        [BelowRichLabel("$" + nameof(skin)), SpineSkin]
        public string skin;

        [SpineSkinPicker]
        public string skinPicked;
    }
}
