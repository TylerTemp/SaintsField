using UnityEngine;

namespace SaintsField.Samples.Scripts.BelowImageWithSubExample
{
    public class BelowImageWithSub : MonoBehaviour
    {
        // field object, then find the target in hierarchy
        [FieldInfoBox("Show Image under subPrefab/SR")]
        [BelowImage("./SR", maxWidth: 40)] public GameObject subPrefab;

        // find the target in current object's hierarchy
        [FieldInfoBox("Show Image under current GameObject/SubWithSpriteRenderer/SR")]
        [BelowImage("/SubWithSpriteRenderer/SR", maxWidth: 40)] public string thisSub;
    }
}
