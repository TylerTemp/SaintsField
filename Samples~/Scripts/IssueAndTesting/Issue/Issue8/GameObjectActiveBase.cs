using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class GameObjectActiveBase : MonoBehaviour
    {
        [SerializeField, GameObjectActive] private GameObject go;
        [SerializeField, GameObjectActive] private GameObjectActiveBase component;

    }
}
