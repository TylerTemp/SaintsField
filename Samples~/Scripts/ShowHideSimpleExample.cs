using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ShowHideSimpleExample: MonoBehaviour
    {
        public bool bool1;
        [ShowIf(nameof(bool1))]
        [RichLabel("<color=red>show")]
        public string showBool;
    }
}
