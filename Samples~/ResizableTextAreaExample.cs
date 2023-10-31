using UnityEngine;

namespace ExtInspector.Samples
{
    public class ResizableTextAreaExample : MonoBehaviour
    {
        [SerializeField, ResizableTextArea] private string _short;
        [SerializeField, ResizableTextArea] private string _long;
        [SerializeField, RichLabel(null), ResizableTextArea] private string _noLabel;

        [SerializeField, NaughtyAttributes.ResizableTextArea]
        private string _nat;

        [SerializeField, TextArea] private string _unity;
    }
}
