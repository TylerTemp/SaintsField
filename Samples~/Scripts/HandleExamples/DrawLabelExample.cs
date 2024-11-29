using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class DrawLabelExample : MonoBehaviour
    {
        [DrawLabel("Test"), GetComponent]
        public GameObject thisObj;

        [Serializable]
        public enum MonsterState
        {
            Idle,
            Attacking,
            Dead,
        }

        public MonsterState monsterState;

        [DrawLabel(EColor.Yellow ,"$" + nameof(monsterState))]
        public GameObject child;
    }
}
