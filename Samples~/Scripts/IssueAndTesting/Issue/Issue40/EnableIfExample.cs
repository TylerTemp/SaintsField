using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue40
{
    public class EnableIfExample : MonoBehaviour
    {
        [Range(0, 2)] public int int01;

        [EnableIf(nameof(int01), 1), FieldRichLabel("default")] public string int01Enable1;
        [EnableIf(nameof(int01) + ">", 1), FieldRichLabel(">1")] public string int01EnableGt1;
        [EnableIf(nameof(int01) + ">=", 1), FieldRichLabel(">=1")] public string int01EnableGe1;
        [EnableIf(nameof(int01) + "<", 1), FieldRichLabel("<1")] public string int01EnableLt1;
        [EnableIf(nameof(int01) + "<=", 1), FieldRichLabel("<=1")] public string int01EnableLe1;
        // ReSharper disable once InconsistentNaming
        [EnableIf(nameof(int01) + "!=", 1), FieldRichLabel("!=1")] public string int01EnableNE1;

        [EnableIf("!" + nameof(int01), 1), FieldRichLabel("! ==1")] public string int01EnableN1;
        [EnableIf("!" + nameof(int01) + ">", 1), FieldRichLabel("! >1")] public string int01EnableNGt1;
        [EnableIf("!" + nameof(int01) + ">=", 1), FieldRichLabel("! >=1")] public string int01EnableNGe1;
        [EnableIf("!" + nameof(int01) + "<", 1), FieldRichLabel("! <1")] public string int01EnableNLt1;
        [EnableIf("!" + nameof(int01) + "<=", 1), FieldRichLabel("! <=1")] public string int01EnableNLe1;
        // ReSharper disable once InconsistentNaming
        [EnableIf("!" + nameof(int01) + "!=", 1), FieldRichLabel("! !=1")] public string int01EnableNNE1;

        [Space]
        [Range(0, 2)] public int int02;

        [EnableIf(nameof(int01) + "==$", nameof(int02)), FieldRichLabel("==$")] public string int01Enable1Callback;
        [EnableIf(nameof(int01) + ">$", nameof(int02)), FieldRichLabel(">$")] public string int01EnableGt1Callback;
        [EnableIf(nameof(int01) + ">=$", nameof(int02)), FieldRichLabel(">=$")] public string int01EnableGe1Callback;
        [EnableIf(nameof(int01) + "<$", nameof(int02)), FieldRichLabel("<$")] public string int01EnableLt1Callback;
        [EnableIf(nameof(int01) + "<=$", nameof(int02)), FieldRichLabel("<=$")] public string int01EnableLe1Callback;
        // ReSharper disable once InconsistentNaming
        [EnableIf(nameof(int01) + "!=$", nameof(int02)), FieldRichLabel("!=$")] public string int01EnableNE1Callback;

        [EnableIf("!" + nameof(int01) + "==$", nameof(int02)), FieldRichLabel("! ==$")] public string int01EnableN1Callback;
        [EnableIf("!" + nameof(int01) + ">$", nameof(int02)), FieldRichLabel("! >$")] public string int01EnableNGt1Callback;
        [EnableIf("!" + nameof(int01) + ">=$", nameof(int02)), FieldRichLabel("! >=$")] public string int01EnableNGe1Callback;
        [EnableIf("!" + nameof(int01) + "<$", nameof(int02)), FieldRichLabel("! <&")] public string int01EnableNLt1Callback;
        [EnableIf("!" + nameof(int01) + "<=$", nameof(int02)), FieldRichLabel("! <=$")] public string int01EnableNLe1Callback;
        // ReSharper disable once InconsistentNaming
        [EnableIf("!" + nameof(int01) + "!=$", nameof(int02)), FieldRichLabel("! !=$")] public string int01EnableNNE1Callback;

        public bool boolValue;

        [Space]
        [EnableIf(nameof(boolValue)), FieldRichLabel("default")] public string boolValueEnable;
        [EnableIf("!" + nameof(boolValue)), FieldRichLabel("! ")] public string boolValueEnableN;

        [Flags, Serializable]
        public enum EnumF
        {
            A = 1,
            B = 1 << 1,
        }

        [Space]
        [EnumToggleButtons]
        public EnumF enumF;

        [EnableIf(nameof(enumF), EnumF.A), FieldRichLabel("hasFlag(A)")] public string enumFEnableA;
        [EnableIf(nameof(enumF), EnumF.B), FieldRichLabel("hasFlag(B)")] public string enumFEnableB;
        // ReSharper disable once InconsistentNaming
        [EnableIf(nameof(enumF), EnumF.A | EnumF.B), FieldRichLabel("hasFlag(A | B)")] public string enumFEnableAB;

        [Serializable]
        public enum EnumOnOff
        {
            A = 1,
            B = 1 << 1,
        }

        [Space]
        public EnumOnOff onOff;
        [EnableIf(nameof(onOff), EnumOnOff.A), FieldRichLabel("A")] public string onOffEnableOn;
        [EnableIf(nameof(onOff), EnumOnOff.B), FieldRichLabel("B")] public string onOffEnableOff;

        [Space]
        [Range(0, 3)] public int enumOnOffBits;

        [EnableIf(nameof(enumOnOffBits) + "&", EnumOnOff.A), FieldRichLabel("&01")] public string bitA;
        [EnableIf(nameof(enumOnOffBits) + "^", EnumOnOff.B), FieldRichLabel("^10")] public string bitB;
        [EnableIf(nameof(enumOnOffBits) + "&==", EnumOnOff.B), FieldRichLabel("hasFlag(B)")] public string hasFlagB;
    }
}
