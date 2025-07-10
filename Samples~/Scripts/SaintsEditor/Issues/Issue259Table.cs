using System;
using SaintsField.ComponentHeader;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue259Table : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public GameObject prefab;
            public ScriptableObject activateThis;
            public int count;
            public bool megaWave;
        }

        [Table] public MyStruct[] waves;

        // ReSharper disable once InconsistentNaming
        public int _nextSpawnTime;

        [Button("NEXT WAVE <color=red>>"), HeaderButton("<color=red>>","Next wave")]
        public void DEBUG_TRIGGER_WAVE() => _nextSpawnTime = 0;
    }
}
