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
        [NavMeshArea, OnValueChanged(nameof(AreaSingleMaskChanged)), AboveButton(nameof(ResetZero)), RichLabel("<icon=star.png /><label />")]
#else
        [InfoBox("Ai Navigation is not installed or enabled", EMessageType.Error)]
#endif
        public int areaSingleMask;

#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
        [NavMeshArea(false), OnValueChanged(nameof(AreaValueChanged))]
#else
        [InfoBox("Ai Navigation is not installed or enabled", EMessageType.Error)]
#endif
        public int areaValue;

#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
        [NavMeshArea, OnValueChanged(nameof(AreaNameChanged))]
#else
        [InfoBox("Ai Navigation is not installed or enabled", EMessageType.Error)]
#endif
        public int areaName;

#if SAINTSFIELD_AI_NAVIGATION
        private void AreaSingleMaskChanged() => Debug.Log($"areaMask: {areaSingleMask}");
        private void AreaValueChanged() => Debug.Log($"areaValue: {areaValue}");
        private void AreaNameChanged() => Debug.Log($"areaName: {areaName}");
        private void ResetZero()
        {
            areaSingleMask = 0;
            areaValue = 0;
            areaMask = 0;
        }
#endif

        [ReadOnly]
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
        [NavMeshArea]
#else
        [InfoBox("Ai Navigation is not installed or enabled", EMessageType.Error)]
#endif
        public int areaNameReadonly;

#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
        [NavMeshAreaMask, OnValueChanged(nameof(AreaMaskChanged)), RichLabel("<icon=star.png /><label />")]
#else
        [InfoBox("Ai Navigation is not installed or enabled", EMessageType.Error)]
#endif
        [Space]
        public int areaMask;

#if SAINTSFIELD_AI_NAVIGATION
        private void AreaMaskChanged() => Debug.Log($"areaMask: {areaMask}");
#endif

        [ReadOnly]
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
        [NavMeshAreaMask]
#else
        [InfoBox("Ai Navigation is not installed or enabled", EMessageType.Error)]
#endif
        public int areaMaskReadonly;
    }
}
