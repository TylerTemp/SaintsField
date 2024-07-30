using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class FieldTypeExample: MonoBehaviour
    {
        [SerializeField, FieldType(typeof(Dummy))][RichLabel("<icon=star.png /><label />")]
        private GameObject _go;

        [SerializeField, FieldType(typeof(SpriteRenderer))]
        private GameObject _sr;

        [SerializeField, FieldType(typeof(Collider))]
        private Dummy dummy;

        [ReadOnly]
        [SerializeField, FieldType(typeof(Collider))]
        private Dummy dummyDisabled;

        [FieldType(EPick.Assets)] public Dummy dummyPrefab;
        [FieldType(EPick.Assets)] public Dummy[] dummyPrefabs;
    }
}
