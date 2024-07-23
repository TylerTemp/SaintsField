using UnityEngine;

namespace SaintsField.Samples.Scripts.Separator
{
    public class SeparatorParent : MonoBehaviour
    {
        [BelowSeparator("End Of <b><color=Aqua><containerType/></color></b>", EAlign.Center, space: 10)]
        public string parent;
    }

}
