using UnityEngine;

#if SAINTSFIELD_AI_NAVIGATION
using SaintsField.AiNavigation;
using UnityEngine.AI;
#endif

namespace SaintsField.Samples.Scripts
{
    public class NavMeshExample : MonoBehaviour
    {
#if SAINTSFIELD_AI_NAVIGATION
        [NavMeshArea, OnValueChanged(nameof(AreaMaskChanged)), AboveButton(nameof(ResetZero))]
#endif
        public int areaMask;

#if SAINTSFIELD_AI_NAVIGATION
        [NavMeshArea(false), OnValueChanged(nameof(AreaValueChanged))]
#endif
        public int areaValue;

#if SAINTSFIELD_AI_NAVIGATION
        private void AreaMaskChanged() => Debug.Log($"areaMask: {areaMask}");
        private void AreaValueChanged() => Debug.Log($"areaValue: {areaValue}");
        private void ResetZero()
        {
            areaMask = 0;
            areaValue = 0;
            navMeshAreaMask = 0;
        }
#endif

        #if SAINTSFIELD_AI_NAVIGATION
        [NavMeshAreaMask]
        #endif
        public int navMeshAreaMask;
    }
}
