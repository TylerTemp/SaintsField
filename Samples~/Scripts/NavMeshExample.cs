#if SAINTSFIELD_AI_NAVIGATION
using SaintsField.AiNavigation;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class NavMeshExample : MonoBehaviour
    {
#if SAINTSFIELD_AI_NAVIGATION
        [NavMeshArea]
#endif
        public int navMeshArea;
    }
}
