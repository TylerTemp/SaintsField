using UnityEngine;
using SaintsField.AiNavigation;

#if SAINTSFIELD_AI_NAVIGATION
using UnityEngine.AI;
#endif

namespace SaintsField.Samples.Scripts
{
    public class NavMeshExample : MonoBehaviour
    {
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
        [NavMeshArea, OnValueChanged(nameof(AreaSingleMaskChanged)), AboveButton(nameof(ResetZero)), FieldLabelText("<icon=star.png /><label />")]
        public int areaSingleMask;

        [NavMeshArea(false), OnValueChanged(nameof(AreaValueChanged))]
        public int areaValue;

        [NavMeshArea, OnValueChanged(nameof(AreaNameChanged))]
        public int areaName;

        private void AreaSingleMaskChanged() => Debug.Log($"areaMask: {areaSingleMask}");
        private void AreaValueChanged() => Debug.Log($"areaValue: {areaValue}");
        private void AreaNameChanged() => Debug.Log($"areaName: {areaName}");
        private void ResetZero()
        {
            areaSingleMask = 0;
            areaValue = 0;
            areaMask = 0;
        }

        [FieldReadOnly]
        [NavMeshArea]
        public int areaNameReadonly;

        [NavMeshAreaMask, OnValueChanged(nameof(AreaMaskChanged)), FieldLabelText("<icon=star.png /><label />")]
        [Space]
        public int areaMask;

        private void AreaMaskChanged() => Debug.Log($"areaMask: {areaMask}");


        [FieldReadOnly]
        [InfoBox("Ai Navigation is not installed or enabled", EMessageType.Error)]
        public int areaMaskReadonly;
#endif
    }
}
