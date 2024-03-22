using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class FieldTypeExample: MonoBehaviour
    {
        [SerializeField, FieldType(typeof(Dummy))][RichLabel("<label />")]
        private GameObject _go;

        [SerializeField, FieldType(typeof(SpriteRenderer))]
        private GameObject _sr;

        [SerializeField, FieldType(typeof(Collider))]
        private Dummy dummy;

        // public string normal;
    }
}
