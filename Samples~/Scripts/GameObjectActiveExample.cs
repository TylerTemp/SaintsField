using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GameObjectActiveExample : MonoBehaviour
    {
        [SerializeField, GameObjectActive] private GameObject _go;
        [SerializeField, GameObjectActive] private GameObjectActiveExample _component;
        [ReadOnly] [SerializeField, GameObjectActive] private GameObjectActiveExample _componentDisabled;
    }
}
