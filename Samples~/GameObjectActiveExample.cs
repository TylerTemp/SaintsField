using UnityEngine;

namespace SaintsField.Samples
{
    public class GameObjectActiveExample : MonoBehaviour
    {
        [SerializeField, GameObjectActive] private GameObject _go;
        [SerializeField, GameObjectActive] private GameObjectActiveExample _component;
    }
}
