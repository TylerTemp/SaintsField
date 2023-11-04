using UnityEngine;

namespace SaintsField.Samples
{
    public class InfoBoxExample : MonoBehaviour
    {
        [field: SerializeField] private bool _show;

        [SerializeField]
        [InfoBox("Hi\ncontent content content content content content content content content content content content content content content content content content content content content content content content content", EMessageType.None, above: true)]
        [InfoBox("Hi\n toggle content content content content content content content content content content content content content content content content content content content content content content", EMessageType.Info, nameof(_show), above: true)]
        [InfoBox("Hi\nby toggle", EMessageType.Info, nameof(_show))]
        [InfoBox(nameof(DynamicMessage), EMessageType.None, contentIsCallback: true)]
        [RichLabel("<color=green>x</color>")]
        private int _content;
        [SerializeField]
        private float _float;

        private (EMessageType, string) DynamicMessage => _content < 0 ? (EMessageType.Warning, $"Negative: {_content}") : (EMessageType.None, $"Value={_content}");
    }
}
