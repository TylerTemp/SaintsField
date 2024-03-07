using UnityEngine;

namespace SaintsField.Samples.Scripts.CustomPropDrawer
{
    public class CustomPropExample: MonoBehaviour
    {
        [AboveRichLabel("Above")]
        [InfoBox("Below")]
        [CustomProp]
        [SepTitle(EColor.Gray)]
        public string sth;
    }
}
