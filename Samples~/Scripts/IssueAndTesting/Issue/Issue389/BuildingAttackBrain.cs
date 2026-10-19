using System;
using UnityEngine;
#if UNITY_EDITOR
using SaintsField.Editor.Utils;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue389
{
    [Serializable]
    public class BuildingAttackBrain
    {
        [EnableIfParentAs(typeof(BuildObject))]
        [InfoBox("Only available in BuildObject type", show: "..@not-as(BuildObject)")]
        public int damage;
    }

    public class EnableIfParentAsAttribute: EnableIfAttribute
    {
        public EnableIfParentAsAttribute(Type type) : base("..@as(" + type.FullName + ")")
        {
            Debug.Log(type.FullName);
        }
    }
}
