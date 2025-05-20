using AK.Wwise;
using MaJiang.Utils;
using SaintsField.Wwise;

namespace SaintsField.Samples.Scripts.Wwise
{
    public class GetBankExample : SaintsMonoBehaviour
    {
        [GetBank("BGM*")]
        public Bank bank;

        [GetBank("*BGM*")]
        public Bank[] banks;
    }
}
