using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class InfoBoxExample : MonoBehaviour
    {
        [field: SerializeField] private bool _show;

        // [Space]
        [FieldInfoBox("Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content", EMessageType.None)]
        [FieldInfoBox("$" + nameof(DynamicMessage), EMessageType.Warning)]
        [FieldBelowInfoBox("$" + nameof(DynamicMessageWithIcon))]
        [FieldBelowInfoBox("Hi\n toggle content ", EMessageType.Info, nameof(_show))]
        public bool _content;

        private (EMessageType, string) DynamicMessageWithIcon => _content ? (EMessageType.Error, "False!") : (EMessageType.None, "True!");
        private string DynamicMessage() => _content ? "False" : "True";

        [FieldInfoBox("\\$This is not a callback by using \\ to escape $")]
        public int _modInt;

        public int _defaultInt;

        [FieldInfoBox("Can not be sold", show: nameof(canSell))]
        [LeftToggle]
        [FieldAboveText(nameof(canSell), isCallback:true)]
        public bool canSell;

        [ReadOnly]
        [FieldInfoBox("Can not be sold")]
        public bool canSellDisabled;
    }
}
