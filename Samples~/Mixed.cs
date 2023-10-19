using ExtInspector.Standalone;
// using NaughtyAttributes;
using UnityEngine;

namespace Samples
{
    public class Mixed : MonoBehaviour
    {
        [SerializeField]
        [MinMaxSlider(0f, 1f)]
        // [Range(1, 100)]
        [RichLabel("<color=red>MinMaxSlider</color>")]
        // [Label("WTF")]
        private Vector2 _mixed;
    }
}
