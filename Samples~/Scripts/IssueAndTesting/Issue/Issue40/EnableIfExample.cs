using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue40
{
    public class EnableIfExample : MonoBehaviour
    {
        [Range(0, 2)] public int int01;

        [FieldEnableIf(nameof(int01), 1), FieldLabelText("default")] public string int01Enable1;
        [FieldEnableIf(nameof(int01) + ">", 1), FieldLabelText(">1")] public string int01EnableGt1;
        [FieldEnableIf(nameof(int01) + ">=", 1), FieldLabelText(">=1")] public string int01EnableGe1;
        [FieldEnableIf(nameof(int01) + "<", 1), FieldLabelText("<1")] public string int01EnableLt1;
        [FieldEnableIf(nameof(int01) + "<=", 1), FieldLabelText("<=1")] public string int01EnableLe1;
        // ReSharper disable once InconsistentNaming
        [FieldEnableIf(nameof(int01) + "!=", 1), FieldLabelText("!=1")] public string int01EnableNE1;

        [FieldEnableIf("!" + nameof(int01), 1), FieldLabelText("! ==1")] public string int01EnableN1;
        [FieldEnableIf("!" + nameof(int01) + ">", 1), FieldLabelText("! >1")] public string int01EnableNGt1;
        [FieldEnableIf("!" + nameof(int01) + ">=", 1), FieldLabelText("! >=1")] public string int01EnableNGe1;
        [FieldEnableIf("!" + nameof(int01) + "<", 1), FieldLabelText("! <1")] public string int01EnableNLt1;
        [FieldEnableIf("!" + nameof(int01) + "<=", 1), FieldLabelText("! <=1")] public string int01EnableNLe1;
        // ReSharper disable once InconsistentNaming
        [FieldEnableIf("!" + nameof(int01) + "!=", 1), FieldLabelText("! !=1")] public string int01EnableNNE1;

        [Space]
        [Range(0, 2)] public int int02;

        [FieldEnableIf(nameof(int01) + "==$", nameof(int02)), FieldLabelText("==$")] public string int01Enable1Callback;
        [FieldEnableIf(nameof(int01) + ">$", nameof(int02)), FieldLabelText(">$")] public string int01EnableGt1Callback;
        [FieldEnableIf(nameof(int01) + ">=$", nameof(int02)), FieldLabelText(">=$")] public string int01EnableGe1Callback;
        [FieldEnableIf(nameof(int01) + "<$", nameof(int02)), FieldLabelText("<$")] public string int01EnableLt1Callback;
        [FieldEnableIf(nameof(int01) + "<=$", nameof(int02)), FieldLabelText("<=$")] public string int01EnableLe1Callback;
        // ReSharper disable once InconsistentNaming
        [FieldEnableIf(nameof(int01) + "!=$", nameof(int02)), FieldLabelText("!=$")] public string int01EnableNE1Callback;

        [FieldEnableIf("!" + nameof(int01) + "==$", nameof(int02)), FieldLabelText("! ==$")] public string int01EnableN1Callback;
        [FieldEnableIf("!" + nameof(int01) + ">$", nameof(int02)), FieldLabelText("! >$")] public string int01EnableNGt1Callback;
        [FieldEnableIf("!" + nameof(int01) + ">=$", nameof(int02)), FieldLabelText("! >=$")] public string int01EnableNGe1Callback;
        [FieldEnableIf("!" + nameof(int01) + "<$", nameof(int02)), FieldLabelText("! <&")] public string int01EnableNLt1Callback;
        [FieldEnableIf("!" + nameof(int01) + "<=$", nameof(int02)), FieldLabelText("! <=$")] public string int01EnableNLe1Callback;
        // ReSharper disable once InconsistentNaming
        [FieldEnableIf("!" + nameof(int01) + "!=$", nameof(int02)), FieldLabelText("! !=$")] public string int01EnableNNE1Callback;

        public bool boolValue;

        [Space]
        [FieldEnableIf(nameof(boolValue)), FieldLabelText("default")] public string boolValueEnable;
        [FieldEnableIf("!" + nameof(boolValue)), FieldLabelText("! ")] public string boolValueEnableN;

        [Flags, Serializable]
        public enum EnumF
        {
            A = 1,
            B = 1 << 1,
        }

        [Space]
        [EnumToggleButtons]
        public EnumF enumF;

        [FieldEnableIf(nameof(enumF), EnumF.A), FieldLabelText("hasFlag(A)")] public string enumFEnableA;
        [FieldEnableIf(nameof(enumF), EnumF.B), FieldLabelText("hasFlag(B)")] public string enumFEnableB;
        // ReSharper disable once InconsistentNaming
        [FieldEnableIf(nameof(enumF), EnumF.A | EnumF.B), FieldLabelText("hasFlag(A | B)")] public string enumFEnableAB;

        [Serializable]
        public enum EnumOnOff
        {
            A = 1,
            B = 1 << 1,
        }

        [Space]
        public EnumOnOff onOff;
        [FieldEnableIf(nameof(onOff), EnumOnOff.A), FieldLabelText("A")] public string onOffEnableOn;
        [FieldEnableIf(nameof(onOff), EnumOnOff.B), FieldLabelText("B")] public string onOffEnableOff;

        [Space]
        [Range(0, 3)] public int enumOnOffBits;

        [FieldEnableIf(nameof(enumOnOffBits) + "&", EnumOnOff.A), FieldLabelText("&01")] public string bitA;
        [FieldEnableIf(nameof(enumOnOffBits) + "^", EnumOnOff.B), FieldLabelText("^10")] public string bitB;
        [FieldEnableIf(nameof(enumOnOffBits) + "&==", EnumOnOff.B), FieldLabelText("hasFlag(B)")] public string hasFlagB;
    }
}
