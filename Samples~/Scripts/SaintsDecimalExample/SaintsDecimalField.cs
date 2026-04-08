using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDecimalExample
{
    public class SaintsDecimalField : MonoBehaviour
    {
        public SaintsDecimal saintsDecimal;

        private void Awake()
        {
            // implicit converting supported
            decimal convertToDecimal = saintsDecimal;
            decimal add = decimal.One + saintsDecimal;
        }
    }
}
