using UnityEngine;

namespace ExtInspector
{
    public class FieldTypeExample: MonoBehaviour
    {
        [SerializeField, FieldType(typeof(SpriteRenderer))]
        private GameObject _go;

        [SerializeField, FieldType(typeof(GameObject))]
        private ParticleSystem _ps;
    }
}
