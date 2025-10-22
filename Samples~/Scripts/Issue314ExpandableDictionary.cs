using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class Issue314ExpandableDictionary : SaintsMonoBehaviour
    {
        [SaintsDictionary(keyWidth: "20%")]
        [ValueAttribute(typeof(ExpandableAttribute))]
        public SaintsDictionary<int, Collider> expand;
    }
}
