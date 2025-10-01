using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue308OrderMerged : SaintsMonoBehaviour
    {
        public GameObject  LFireball
            ,RFireball
            ,MFireballRig
            ,MFireball
            ,MFireballReady;
        public Transform             fizzleVFX;
        public Transform         launcher;
        public float                      megafireballDistanceThreshold = .1f;
        float                             DistanceThreshold => megafireballDistanceThreshold*Mathf.Max(1,2);
        [Expandable] public GameObject joinHandSO;
        float                             timer;
        public float                      maxFireballScale = 1;
        public float maxTime   = 5
            ,maxDamage = 20;
        public List<GameObject> lightnings;
        public AnimationCurve          curve;
        public float                   minScale = .2f;
    }
}
