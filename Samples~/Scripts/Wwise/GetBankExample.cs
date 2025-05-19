using AK.Wwise;
using SaintsField.Wwise;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Wwise
{
    public class GetBankExample : MonoBehaviour
    {
        [GetBank("//BGMMain")]
        public Bank bank;
    }
}
