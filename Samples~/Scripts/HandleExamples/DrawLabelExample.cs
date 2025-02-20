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

        [DrawLabel("$" + nameof(monsterState), color: nameof(color))]
        public GameObject child;

        public Color color;

        [GetComponentInChildren, DrawLabel(color: nameof(color))] public Transform[] getChildren;
    }
}
