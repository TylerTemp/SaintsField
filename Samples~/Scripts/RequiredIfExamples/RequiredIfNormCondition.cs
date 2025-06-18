using UnityEngine;

namespace SaintsField.Samples.Scripts.RequiredIfExamples
{
    public class RequiredIfNormCondition : MonoBehaviour
    {
        [Separator("Depende on other field or callback")]
        public GameObject go;
        [RequiredIf(nameof(go))]  // if a field is a dependence of another field
        public GameObject requiredIfGo;

        public int intValue;
        [RequiredIf(nameof(intValue) + ">=", 0)]
        public GameObject requiredIfPositive;  // if meet some condition; callback is also supported.

        [Separator("EMode condition")]

        [RequiredIf(EMode.InstanceInScene)]
        public GameObject sceneObj;  // if it's a prefab in a scene

        [Separator("Suggestion")]

        // use as a notice
        public Transform hand;
        [RequiredIf(nameof(hand))]
        [Required("It's suggested to set this field if 'hand' is set", EMessageType.Info)]
        public GameObject suggestedIfHand;

        [Separator("And")]

        // You can also chain multiple conditions as "and" operation
        public GameObject andCondition;
        [RequiredIf(EMode.InstanceInScene, nameof(andCondition))]
        public GameObject instanceInSceneAndCondition;  // if it's a prefab in a scene and 'andCondition' is set

        [Separator("Or")]

        // You can also chain multiple RequiredIf as "or" operation
        public GameObject orCondition;
        public int orValue;
        [RequiredIf(nameof(orCondition))]
        [RequiredIf(nameof(orValue) + ">=", 0)]
        public GameObject requiredOr;  // if it's a prefab in a scene and 'andCondition' is set
    }
}
