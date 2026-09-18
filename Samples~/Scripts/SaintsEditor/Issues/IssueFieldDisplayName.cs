using SaintsField;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class IssueFieldDisplayName : SaintsMonoBehaviour
    {
        [AboveText("<color=gray>Config: <field.displayName />")]
        [SerializeField, Expandable, Required, RichLabel("Config")]
        private IssueFieldDisplayNameConfig config;
    }

}
