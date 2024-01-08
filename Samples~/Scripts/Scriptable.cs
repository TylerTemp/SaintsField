using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    [CreateAssetMenu(fileName = "Scriptable", menuName = "ScriptableObjects/Scriptable", order = 0)]
    public class Scriptable : ScriptableObject
    {
        [SerializeField]
        // [RichLabel("<color=red><label /></color>")]
        [PropRange(0, 100)]
        private int _intRange;

        [Range(0, 100)] public int normalRange;

        public int publicValue;

        [RichLabel(null)]
        public string noLabel;
    }
}
