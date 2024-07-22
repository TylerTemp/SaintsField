using System.Collections;
using System.Collections.Generic;
using SaintsField;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Separator
{
    public class SeparatorParent : MonoBehaviour
    {
        [BelowSeparator("End Of <color=Aqua><typeName/></color>", EAlign.Center, space: 10)]
        public string parent;
    }

}
