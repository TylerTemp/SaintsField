using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class InfoBoxExample : MonoBehaviour
    {
        [field: SerializeField] private bool _show;

        [Space]
        [InfoBox("Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content", EMessageType.None, above: true)]
        [InfoBox(nameof(DynamicMessage), EMessageType.Warning, contentIsCallback: true, above: true)]
        [InfoBox(nameof(DynamicMessageWithIcon), contentIsCallback: true)]
        [InfoBox("Hi\n toggle content ", EMessageType.Info, nameof(_show))]
        public bool _content;

        private (EMessageType, string) DynamicMessageWithIcon => _content ? (EMessageType.Error, "False!") : (EMessageType.None, "True!");
        private string DynamicMessage() => _content ? "False" : "True";

        [InfoBox("does not modify label & field")]
        public int _modInt;

        public int _defaultInt;
    }
}
