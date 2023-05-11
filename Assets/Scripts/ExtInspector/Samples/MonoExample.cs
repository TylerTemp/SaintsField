using ExtInspector.Standalone;
using UnityEngine;

namespace ExtInspector.Samples
{
    public class MonoExample : MonoBehaviour
    {
        [SerializeField, Scene] private string _scene;
        [GO] public GameObject _go;
    }
}
