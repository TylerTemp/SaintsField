using AK.Wwise;
using MaJiang.Utils;
using SaintsField.Wwise;

namespace SaintsField.Samples.Scripts.Wwise
{
    public class GetBankExample : SaintsMonoBehaviour
    {
        [GetWwise("BGM*")]
        public Bank bank;

        [GetWwise("*BGM*")]
        public Event[] events;

        [GetWwise]
        public RTPC rtpc;

        [GetWwise] public string incorrectType;

        [GetWwise("*/BGM/Stop*")]
        public Event[] stopEvents;
    }
}
