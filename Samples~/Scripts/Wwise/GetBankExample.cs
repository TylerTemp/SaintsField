using AK.Wwise;
using MaJiang.Utils;
using SaintsField.Wwise;
using Event = AK.Wwise.Event;

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

//         [Serializable]
//         public class BGMSwitchDictioary: SaintsDictionaryBase<int, Switch>
//         {
//             protected override List<Wrap<int>> SerializedKeys => _saintsKeys;
//             protected override List<Wrap<Switch>> SerializedValues => _saintsValues;
//
// #if UNITY_EDITOR
//             // ReSharper disable once UnusedMember.Local
//             private static string EditorPropKeys => nameof(_saintsKeys);
//             // ReSharper disable once UnusedMember.Local
//             private static string EditorPropValues => nameof(_saintsValues);
// #endif
//
//             [SerializeField] private List<Wrap<int>> _saintsKeys = new List<Wrap<int>>();
//
// #if UNITY_EDITOR
//             [GetWwise("$" + nameof(EditorGetSwitch))]
// #endif
//             [SerializeField] private List<Wrap<Switch>> _saintsValues = new List<Wrap<Switch>>();
//
// #if UNITY_EDITOR
//             private string EditorGetSwitch(int index)
//             {
//                 return $"//Switch{index}";
//             }
// #endif
//         }
//
//         [DefaultExpand] public BGMSwitchDictioary bgmSwitchDict;

    }
}
