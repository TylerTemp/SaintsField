using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetPrefabWithComponentExample: MonoBehaviour
    {
        [GetPrefabWithComponent] public Dummy myDummy;
        // get the prefab itself
        [GetPrefabWithComponent(compType: typeof(Dummy))] public GameObject myDummyPrefab;
        // works so good with `FieldType`
        [GetPrefabWithComponent(compType: typeof(Dummy)), FieldType(typeof(Dummy))] public GameObject myDummyPrefabFieldType;
    }
}
