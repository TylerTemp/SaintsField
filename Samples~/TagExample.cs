using UnityEngine;

namespace ExtInspector.Samples
{
    public class TagExample: MonoBehaviour
    {
        [SerializeField, Tag] private string _tag;
    }
}
