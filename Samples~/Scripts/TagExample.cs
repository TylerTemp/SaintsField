using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class TagExample: MonoBehaviour
    {
        [SerializeField, Tag] private string _tag;
        [SerializeField, Tag][RichLabel(null)] private string _tag2;
    }
}
