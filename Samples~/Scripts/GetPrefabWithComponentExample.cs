using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetPrefabWithComponentExample: MonoBehaviour
    {
        [GetPrefabWithComponent] public Dummy dummy;
        [GetPrefabWithComponent(compType: typeof(Dummy))] public GameObject dummyPrefab;
    }
}
