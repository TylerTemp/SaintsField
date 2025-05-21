using AK.Wwise;
using MaJiang.Utils;
using SaintsField.Wwise;
using UnityEngine;
using Event = AK.Wwise.Event;

namespace SaintsField.Samples.Scripts.Wwise
{
    public class GetBankExample : SaintsMonoBehaviour
    {
        [SerializeField, GetWwise("PlayBGM")] private AK.Wwise.Event _playEvent;

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
