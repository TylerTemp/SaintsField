#if SAINTSFIELD_NETCODE_GAMEOBJECTS && !SAINTSFIELD_NETCODE_GAMEOBJECTS_DISABLED
using SaintsField.Playa;
using Unity.Netcode;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.NetCode
{
    public class RpcTestDefault : NetworkBehaviour
    {
        public string normalString;

        [ResizableTextArea, InfoBox("SainsField")]
        public string content;

        [Button]
        private void TestRpc()
        {
            Debug.Log("Button Invoked");
        }

        public NetworkVariable<int> testVar = new NetworkVariable<int>(0);
        public NetworkList<bool> TestList = new NetworkList<bool>();
    }
}

#endif
