using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue286
{
    [Serializable]
// #if SAINTSFIELD_DEBUG
//     [CreateAssetMenu(fileName = "Issue286Scriptable", menuName = "Scriptable Objects/Issue286Scriptable")]
// #endif
    public class Issue286Scriptable : SaintsScriptableObject
    {
        [SerializeField] FebucciUIStyles[] styles = Array.Empty<FebucciUIStyles>();

        [TextArea]
        public string[] ta;
    }
}
