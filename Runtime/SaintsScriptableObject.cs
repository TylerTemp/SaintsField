using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.SaintsSerialization;
using UnityEngine;

namespace SaintsField
{
    public class SaintsScriptableObject: ScriptableObject, ISerializationCallbackReceiver
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

        }

        public void OnAfterDeserialize()
        {
        }
    }
}
