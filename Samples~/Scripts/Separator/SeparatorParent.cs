using UnityEngine;

namespace SaintsField.Samples.Scripts.Separator
{
    public class SeparatorParent : MonoBehaviour
    {
        [BelowSeparator("End Of <b><color=Aqua><typeName/></color></b>", EAlign.Center, space: 10)]
        public string parent;
    }

}
