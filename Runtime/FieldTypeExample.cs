using UnityEngine;

namespace SaintsField
{
    public class FieldTypeExample: MonoBehaviour
    {
        [SerializeField, FieldType(typeof(SpriteRenderer))][RichLabel("<label />")]
        private GameObject _go;

        [SerializeField, FieldType(typeof(FieldTypeExample))]
        private BoxCollider _collider;

        public string normal;
    }
}
