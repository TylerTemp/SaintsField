using UnityEngine;
using UnityEngine.Serialization;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowIfEModeExample : MonoBehaviour
    {
        [EnableIf(EMode.InstanceInScene)]
        public string instanceInScene = "Instances of prefabs in scenes";

        [EnableIf(EMode.InstanceInPrefab)]
        public string instanceInPrefab = "Instances of prefabs nested inside other prefabs";

        [EnableIf(EMode.Regular)]
        public string regular = "Regular prefab assets";

        [EnableIf(EMode.Variant)]
        public string variant = "Prefab variant assets";

        [EnableIf(EMode.NonPrefabInstance)]
        public string nonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

        [EnableIf(EMode.PrefabInstance)]
        public string prefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

        [EnableIf(EMode.PrefabAsset)]
        public string prefabAsset = "Prefab assets and prefab variant assets";
    }
}
