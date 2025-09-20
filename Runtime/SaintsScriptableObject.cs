using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    public partial class SaintsScriptableObject: ScriptableObject, ISerializationCallbackReceiver
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
