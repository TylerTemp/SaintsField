using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SerGuidExample : SaintsMonoBehaviour
    {
        [SaintsSerialized] private Guid _guid;
    }
}
