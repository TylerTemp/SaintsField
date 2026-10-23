using SaintsField;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(menuName = "SaintsField Debug/Issues/Field displayName")]
#endif
    public class IssueFieldDisplayNameConfig : SaintsScriptableObject
    {
        public string displayName;
    }
}
