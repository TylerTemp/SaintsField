using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue40
{
    public class EnableIfExample : MonoBehaviour
    {
        [Range(0, 2)] public int int01;

        [EnableIf(nameof(int01), 1), FieldLabelText("default")] public string int01Enable1;
        [EnableIf(nameof(int01) + ">", 1), FieldLabelText(">1")] public string int01EnableGt1;
        [EnableIf(nameof(int01) + ">=", 1), FieldLabelText(">=1")] public string int01EnableGe1;
        [EnableIf(nameof(int01) + "<", 1), FieldLabelText("<1")] public string int01EnableLt1;
        [EnableIf(nameof(int01) + "<=", 1), FieldLabelText("<=1")] public string int01EnableLe1;
        // ReSharper disable once InconsistentNaming
        [EnableIf(nameof(int01) + "!=", 1), FieldLabelText("!=1")] public string int01EnableNE1;

        [EnableIf("!" + nameof(int01), 1), FieldLabelText("! ==1")] public string int01EnableN1;
        [EnableIf("!" + nameof(int01) + ">", 1), FieldLabelText("! >1")] public string int01EnableNGt1;
        [EnableIf("!" + nameof(int01) + ">=", 1), FieldLabelText("! >=1")] public string int01EnableNGe1;
        [EnableIf("!" + nameof(int01) + "<", 1), FieldLabelText("! <1")] public string int01EnableNLt1;
        [EnableIf("!" + nameof(int01) + "<=", 1), FieldLabelText("! <=1")] public string int01EnableNLe1;
        // ReSharper disable once InconsistentNaming
        [EnableIf("!" + nameof(int01) + "!=", 1), FieldLabelText("! !=1")] public string int01EnableNNE1;

        [Space]
        [Range(0, 2)] public int int02;

        [EnableIf(nameof(int01) + "==$", nameof(int02)), FieldLabelText("==$")] public string int01Enable1Callback;
        [EnableIf(nameof(int01) + ">$", nameof(int02)), FieldLabelText(">$")] public string int01EnableGt1Callback;
        [EnableIf(nameof(int01) + ">=$", nameof(int02)), FieldLabelText(">=$")] public string int01EnableGe1Callback;
        [EnableIf(nameof(int01) + "<$", nameof(int02)), FieldLabelText("<$")] public string int01EnableLt1Callback;
        [EnableIf(nameof(int01) + "<=$", nameof(int02)), FieldLabelText("<=$")] public string int01EnableLe1Callback;
        // ReSharper disable once InconsistentNaming
        [EnableIf(nameof(int01) + "!=$", nameof(int02)), FieldLabelText("!=$")] public string int01EnableNE1Callback;

        [EnableIf("!" + nameof(int01) + "==$", nameof(int02)), FieldLabelText("! ==$")] public string int01EnableN1Callback;
        [EnableIf("!" + nameof(int01) + ">$", nameof(int02)), FieldLabelText("! >$")] public string int01EnableNGt1Callback;
        [EnableIf("!" + nameof(int01) + ">=$", nameof(int02)), FieldLabelText("! >=$")] public string int01EnableNGe1Callback;
        [EnableIf("!" + nameof(int01) + "<$", nameof(int02)), FieldLabelText("! <&")] public string int01EnableNLt1Callback;
        [EnableIf("!" + nameof(int01) + "<=$", nameof(int02)), FieldLabelText("! <=$")] public string int01EnableNLe1Callback;
        // ReSharper disable once InconsistentNaming
        [EnableIf("!" + nameof(int01) + "!=$", nameof(int02)), FieldLabelText("! !=$")] public string int01EnableNNE1Callback;

        public bool boolValue;

        [Space]
        [EnableIf(nameof(boolValue)), FieldLabelText("default")] public string boolValueEnable;
        [EnableIf("!" + nameof(boolValue)), FieldLabelText("! ")] public string boolValueEnableN;

        [Flags, Serializable]
        public enum EnumF
        {
            A = 1,
            B = 1 << 1,
        }

        [Space]
        [EnumToggleButtons]
        public EnumF enumF;

        [EnableIf(nameof(enumF), EnumF.A), FieldLabelText("hasFlag(A)")] public string enumFEnableA;
        [EnableIf(nameof(enumF), EnumF.B), FieldLabelText("hasFlag(B)")] public string enumFEnableB;
        // ReSharper disable once InconsistentNaming
        [EnableIf(nameof(enumF), EnumF.A | EnumF.B), FieldLabelText("hasFlag(A | B)")] public string enumFEnableAB;

        [Serializable]
        public enum EnumOnOff
        {
            A = 1,
            B = 1 << 1,
        }

        [Space]
        public EnumOnOff onOff;
        [EnableIf(nameof(onOff), EnumOnOff.A), FieldLabelText("A")] public string onOffEnableOn;
        [EnableIf(nameof(onOff), EnumOnOff.B), FieldLabelText("B")] public string onOffEnableOff;

        [Space]
        [Range(0, 3)] public int enumOnOffBits;

        [EnableIf(nameof(enumOnOffBits) + "&", EnumOnOff.A), FieldLabelText("&01")] public string bitA;
        [EnableIf(nameof(enumOnOffBits) + "^", EnumOnOff.B), FieldLabelText("^10")] public string bitB;
        [EnableIf(nameof(enumOnOffBits) + "&==", EnumOnOff.B), FieldLabelText("hasFlag(B)")] public string hasFlagB;
    }
}
