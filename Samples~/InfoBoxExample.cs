using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples
{
    public class InfoBoxExample : MonoBehaviour
    {
        [field: SerializeField] private bool _show;

        [SerializeField]
        [InfoBox("Hi\ncontent content content content content content content content content content content content content content content content content content content content content content content content content", MessageType.None, above: true)]
        [InfoBox("Hi\n toggle content content content content content content content content content content content content content content content content content content content content content content", MessageType.Info, nameof(_show), above: true)]
        [InfoBox("Hi\nby toggle", MessageType.Info, nameof(_show))]
        [InfoBox(nameof(DynamicMessage), MessageType.None, contentIsCallback: true)]
        [RichLabel("<color=green>x</color>")]
        private int _content;
        [SerializeField]
        private float _float;

        private (MessageType, string) DynamicMessage => _content < 0 ? (MessageType.Warning, $"Negative: {_content}") : (MessageType.None, $"Value={_content}");
    }
}
