using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowHideSimpleExample: MonoBehaviour
    {
        [HideIf] public string justHide;
        [ShowIf] public string justShow;

        public bool condition;

        [ShowIf(nameof(condition))] public string boolShow;
        [HideIf(nameof(condition))] public string boolHide;
        // [EnableIf] public string noMeaningEnable;
        //
        // public bool bool1;
        // [ShowIf(nameof(bool1))]
        // [RichLabel("<color=red>show")]
        // public string showBool;
        //
        // [ReadOnly(nameof(HasError))] public string error;
        //
        // public bool HasError() => throw new Exception("This is an error callback");
    }
}
