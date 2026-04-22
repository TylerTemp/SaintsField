using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public partial class SerDictPartPrefab : SaintsMonoBehaviour
    {
        [SaintsSerialized] private Dictionary<int, GameObject> _levelToFanParticle;
        // [SerializeField] private SaintsDictionary<int, GameObject> _levelToFanParticleDirect;
        [ShowInInspector] private Dictionary<int, GameObject> _levelToFanParticleShow => _levelToFanParticle;
    }
}
