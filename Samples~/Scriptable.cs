using ExtInspector;
using UnityEngine;

namespace Samples
{
    [CreateAssetMenu(fileName = "Scriptable", menuName = "ScriptableObjects/Scriptable", order = 0)]
    public class Scriptable : ScriptableObject
    {
        [SerializeField]
        [RichLabel("<color=red><label /></color>"), Range(0, 100)]
        private int _intRange;

        public int _publicValue;
    }
}
