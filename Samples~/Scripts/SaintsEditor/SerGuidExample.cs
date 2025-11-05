using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SerGuidExample : SaintsMonoBehaviour
    {
        [SaintsSerialized]
        private Guid _guid;

        [SaintsSerialized]
        private List<Guid> _guidList;

        [ShowInInspector]
        private Guid ShowGuid
        {
            get => _guid;
            set => _guid = value;
        }

        [ShowInInspector, Guid]
        private string ShowGuidString
        {
            get => _guid.ToString();
            set => Guid.TryParse(value, out _guid);
        }
    }
}
