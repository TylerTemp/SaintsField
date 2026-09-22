using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    // https://github.com/TylerTemp/SaintsField/issues/445
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(menuName = "SaintsField Debug/Issue 445 Scene Reference")]
#endif
    public class SceneRef445Asset : ScriptableObject
    {
        [Serializable]
        public struct SomeStruct
        {
            [SerializeField] public SceneReference TheSceneReference;
        }

        [SerializeField] public SomeStruct[] TheStructs = new SomeStruct[1];
    }
}
