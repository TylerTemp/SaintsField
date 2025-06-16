using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowIfEModeExample : MonoBehaviour
    {
        [ShowIf(EMode.InstanceInScene)] public string instanceInScene;
        [ShowIf(EMode.InstanceInPrefab)] public string instanceInPrefab;
        [ShowIf(EMode.Regular)] public string regular;
        [ShowIf(EMode.Variant)] public string variant;
        [ShowIf(EMode.NonPrefabInstance)] public string nonPrefabInstance;
    }
}
