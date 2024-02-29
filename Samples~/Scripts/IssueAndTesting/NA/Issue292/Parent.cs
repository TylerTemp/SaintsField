using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue292
{
    [CreateAssetMenu(fileName = "Scriptable", menuName = "ScriptableObjects/Issue/Parent", order = 0)]
    public class Parent : ScriptableObject
    {
        [Button]
        public virtual void InParent(){}

        [Button]
        public virtual void DoSomething()
        {
            Debug.Log("DoSomething Parent");
        }

        [Button]
        public void InParentNew() => Debug.Log("InParentNew");
    }
}
