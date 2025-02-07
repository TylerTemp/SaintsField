#if SAINTSFIELD_NETCODE_GAMEOBJECTS && !SAINTSFIELD_NETCODE_GAMEOBJECTS_DISABLED
using SaintsField.Playa;
using Unity.Netcode;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.NetCode
{
    public class RpcTestDefault : NetworkBehaviour
    {
        [PlayaInfoBox("Saints Info Box for Array")]
        public int[] normalIntArrays;

        [LayoutStart("SaintsLayout", ELayout.FoldoutBox)]
        public string normalString;

        [ResizableTextArea]
        public string content;

        public NetworkVariable<int> testVar = new NetworkVariable<int>(0);
        public NetworkList<bool> TestList = new NetworkList<bool>();

        [Button]
        private void TestRpc()
        {
            Debug.Log("Button Invoked");
        }
    }
}

#endif
