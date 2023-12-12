using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetPrefabWithComponentExample: MonoBehaviour
    {
        [GetPrefabWithComponent] public Dummy dummy;
        // get the prefab itself
        [GetPrefabWithComponent(compType: typeof(Dummy))] public GameObject dummyPrefab;
        // works so good with `FieldType`
        [GetPrefabWithComponent(compType: typeof(Dummy)), FieldType(typeof(Dummy))] public GameObject dummyPrefabFieldType;
    }
}
