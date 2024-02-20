using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentByPathExample: MonoBehaviour
    {
        [GetComponentByPath(".")] public GameObject currentGameObject;
    }
}
