using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ValueButtonsExample: SaintsMonoBehaviour
    {
        [ValueButtons(nameof(stringItems))] public string clickAButton;
        public List<string> stringItems;

        [ShowInInspector, ValueButtons(nameof(stringItems))]
        public string ShowClickAButton
        {
            get => clickAButton;
            set => clickAButton = value;
        }

        // Use property/field as options
        [GetComponentInChildren] public Transform[] transOpts;
        [ValueButtons(nameof(transOpts))] public Transform transformSelect;

        [Separator]

        // Use a function (list, array, etc.)
        private IEnumerable<Transform> GetTransOpts() => transOpts; // list, array, anything that is IEnumerable
        [ValueButtons(nameof(GetTransOpts))] public Transform transformCallback;

        // Use OptionList for a bit more control
        private OptionList<Transform> GetTransAdvanced()
        {
            OptionList<Transform> result = new OptionList<Transform>
            {
                {transOpts[0].name, transOpts[0]},  // inline add
            };

            // direct add
            result.Add(transOpts[1].name, transOpts[1], true);  // true means disabled
            // rich tags are supported
            result.Add($"<color={EColor.Aquamarine}><icon=star.png/> {transOpts[1].name}", transOpts[2]);
            result.Add(transOpts[3].name, transOpts[3]);
            return result;
        }
        [ValueButtons(nameof(GetTransAdvanced))] public Transform transformAdvanced;

        // Use on bool
        [ValueButtons] public bool myBool;

        // Use on enum
        [Serializable]
        public enum EnumOpt
        {
            First,
            Second,
            Third,
            [InspectorName("<color=lime><label/>")]  // change name is supported
            Forth,
        }
        [ValueButtons] public EnumOpt myEnum;

        // Use to get const/static from type
        [ValueButtons] public Color unityColors;
    }
}
