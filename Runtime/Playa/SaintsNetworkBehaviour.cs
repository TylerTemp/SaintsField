#if SAINTSFIELD_NETCODE_GAMEOBJECTS
using Unity.Netcode;
using System.Collections.Generic;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Playa
{
    public class SaintsNetworkBehaviour: NetworkBehaviour, ISerializationCallbackReceiver
    {
        [PlayaShowIf(
#if SAINTSFIELD_SERIALIZATION_DEBUG
            true
#else
            false
#endif
        )]
        // [ListDrawerSettings(searchable: true)]
        [Table]
        [SerializeField] private List<SaintsSerializedProperty> _saintsSerializedProperties = new List<SaintsSerializedProperty>();

        public void OnBeforeSerialize()
        {
            SaintsSerializedUtil.OnBeforeSerialize(_saintsSerializedProperties, GetType());
        }

        public void OnAfterDeserialize()
        {
            SaintsSerializedUtil.OnAfterDeserialize(_saintsSerializedProperties, GetType(), this);
        }
    }
}
#endif
