using UnityEngine;
using UnityEngine.Serialization;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowIfEModeExample : MonoBehaviour
    {
        [FieldEnableIf(EMode.InstanceInScene)]
        public string instanceInScene = "Instances of prefabs in scenes";

        [FieldEnableIf(EMode.InstanceInPrefab)]
        public string instanceInPrefab = "Instances of prefabs nested inside other prefabs";

        [FieldEnableIf(EMode.Regular)]
        public string regular = "Regular prefab assets";

        [FieldEnableIf(EMode.Variant)]
        public string variant = "Prefab variant assets";

        [FieldEnableIf(EMode.NonPrefabInstance)]
        public string nonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

        [FieldEnableIf(EMode.PrefabInstance)]
        public string prefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

        [FieldEnableIf(EMode.PrefabAsset)]
        public string prefabAsset = "Prefab assets and prefab variant assets";
    }
}
