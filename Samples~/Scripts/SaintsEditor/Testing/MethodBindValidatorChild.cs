using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class MethodBindValidatorChild : MonoBehaviour
    {
        [GetComponentInParents] public MethodBindValidator methodBindParent;

        [OnEvent(nameof(methodBindParent) + ".evt")]
        public void OnChildEvent()
        {
        }

        [OnButtonClick]
        public void OnButtonClick()
        {
        }
    }
}
