using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentByPathExample: MonoBehaviour
    {
        [GetComponentByPath("///sth/else/../what/.//ever[last()]/goes/here")] public GameObject currentGameObject;
    }
}
