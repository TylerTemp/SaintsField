using UnityEngine;

#if SAINTSFIELD_AI_NAVIGATION
using SaintsField.AiNavigation;
using UnityEngine.AI;
#endif

namespace SaintsField.Samples.Scripts
{
    public class NavMeshExample : MonoBehaviour
    {
// #if SAINTSFIELD_AI_NAVIGATION
//         [NavMeshAreaMask]
// #endif
        // [AboveButton("Check")]
        // public int navMeshArea;
        //
        // private void Check()
        // {
        //     Debug.Log(NavMesh.GetAreaFromName("Walkable"));
        //     Debug.Log(NavMesh.GetAreaFromName("Not Walkable"));
        //     Debug.Log(NavMesh.GetAreaFromName("Jump"));
        //     Debug.Log(NavMesh.GetAreaFromName("MyArea1"));
        //     Debug.Log(NavMesh.GetAreaFromName("MyArea2"));
        // }

#if SAINTSFIELD_AI_NAVIGATION
        [NavMeshArea, OnValueChanged(nameof(AreaMaskChanged))]
#endif
        public int areaMask;

#if SAINTSFIELD_AI_NAVIGATION
        [NavMeshArea(false), OnValueChanged(nameof(AreaValueChanged))]
#endif
        public int areaValue;

#if SAINTSFIELD_AI_NAVIGATION
        private void AreaMaskChanged() => Debug.Log($"areaMask: {areaMask}");
        private void AreaValueChanged() => Debug.Log($"areaValue: {areaValue}");
#endif
    }
}
