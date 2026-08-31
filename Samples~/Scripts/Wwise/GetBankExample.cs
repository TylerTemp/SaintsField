using AK.Wwise;
using Event = AK.Wwise.Event;
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

        [DefaultExpand] public SaintsDictionary<int, Switch> bgmSwitchDictioary;

        [GetWwise("Normal")]
        public Switch bgSwitch;
    }
}
