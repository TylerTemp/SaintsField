using UnityEngine;

namespace SaintsField.Samples.Scripts.CustomPropDrawer
{
    public class CustomPropExample: MonoBehaviour
    {
        [FieldAboveText("Above")]
        [FieldInfoBox("Below")]
        [CustomProp]
        [SepTitle(EColor.Gray)]
        public string sth;
    }
}
