using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class EnumToggleButtonsExample: MonoBehaviour
    {
        [Serializable, Flags]
        public enum BitMask
        {
            None = 0,  // this will be replaced for all/none button
            [InspectorName("M<color=red>1</color>")]
            Mask1 = 1,
            [InspectorName("M<color=green>2</color>")]
            Mask2 = 1 << 1,
            [InspectorName("M<color=blue>3</color>")]
            Mask3 = 1 << 2,
            [InspectorName("M4")]
            Mask4 = 1 << 3,
            Mask1To4 = Mask1 | Mask2 | Mask3 | Mask4,
            Mask5 = 1 << 4,
            MaskLongLongLongLong = 1 << 5,
            MaskLongLongLongLong2 = 1 << 6,
            Mask7 = 1 << 7,
            Mask8 = 1 << 8,
        }

        // [RichLabel("<icon=star.png /><label />")]
        [EnumToggleButtons, OnValueChanged(":Debug.Log")]
        public BitMask myMask;

        [EnumToggleButtons(noFold: true), OnValueChanged(":Debug.Log")]
        public BitMask myMaskNoFold;

        [EnumToggleButtons, FieldLabelText(null), OnValueChanged(nameof(ValueChanged))] public BitMask myMask2;
        private void ValueChanged() => Debug.Log(myMask2);

        [Serializable]
        public struct MyStruct
        {
            [EnumToggleButtons, FieldBelowText(nameof(myMask), true)] public BitMask myMask;
        }

        public MyStruct myStruct;

        [FieldReadOnly]
        [EnumToggleButtons]
        public BitMask myMaskDisabled;

        [FieldReadOnly]
        [EnumToggleButtons]
        [FieldLabelText("<icon=star.png /><label />")]
        public BitMask myMaskDisabledLabel;

        [Serializable]
        public enum EnumNormal
        {
            First,
            Second,
            [FieldLabelText("<color=lime><label /></color>")]
            Third,
        }

        [EnumToggleButtons, OnValueChanged(":Debug.Log")] public EnumNormal enumNormal;

        [Serializable]
        public enum EnumExpand
        {
            Value1,
            Value2,
            Value3,
            Value4,
            Value5,
            Value6,
            Value7,
            Value8,
            Value9,
            Value10,
        }

        [EnumToggleButtons, FieldDefaultExpand] public EnumExpand enumExpand;

        [Serializable]
        public enum EnumLabelField
        {
            [FieldLabelText("<color=red><label/></color>")]
            None,
            [FieldLabelText("<color=blue>1st</color> (<label/>)")]
            First,
            [InspectorName("<color=green>2nd</color> (<label/>)")]
            Second,
        }

        [EnumToggleButtons, FieldDefaultExpand] public EnumLabelField labelField;

// #if UNITY_6000_1_OR_NEWER
//         [FieldSeparator("Unity new added")]
//
//         [EnumButtons] public EnumExpand unityEnumE;
//         [EnumButtons] public BitMask unityFlagsE;
// #endif

        [Serializable]
        public enum EnumWithObsolete
        {
            Default,
            One,
            [Obsolete("Example of Obsolete")]
            ObsoletedOption,
        }

        [Serializable, Flags]
        public enum FlagsWithObsolete
        {
            Default = 1,
            One = 1 << 1,
            [Obsolete("Example of Obsolete")]
            ObsoletedOption = 1 << 2,
        }
#if UNITY_6000_1_OR_NEWER
        [EnumButtons] public EnumWithObsolete unityEnumExc;
        [EnumButtons(includeObsolete: true)] public EnumWithObsolete unityEnumInc;
#endif

        [FieldSeparator("SaintsField Obsolete processing")]

        [EnumToggleButtons] public FlagsWithObsolete sFlagExc;
        [EnumToggleButtons(EObsolete.Include)] public FlagsWithObsolete sFlagInc;
        [EnumToggleButtons(EObsolete.Disable)] public FlagsWithObsolete sFlagDis;

        [EnumToggleButtons] public EnumWithObsolete sEnumExc;
        [EnumToggleButtons(EObsolete.Include)] public EnumWithObsolete sEnumInc;
        [EnumToggleButtons(EObsolete.Disable)] public EnumWithObsolete sEnumDis;
        [ValueButtons] public EnumWithObsolete valueButtonsEnumExc;
        [ValueButtons(EObsolete.Include)] public EnumWithObsolete valueButtonsEnumInc;
        [ValueButtons(EObsolete.Disable)] public EnumWithObsolete valueButtonsEnumDis;

        // Both orders reproduce value-to-name ambiguity on different enum implementations.
        // Current and Legacy share a value, but only Legacy is obsolete.
        [Serializable]
        public enum AliasCurrentFirst
        {
            [InspectorName("<color=green><label/></color>")]
            Current = 1,
            [Obsolete("Use Current instead")]
            [InspectorName("<color=orange><label/></color>")]
            Legacy = 1,
            Other = 2,
        }

        [Serializable]
        public enum AliasLegacyFirst
        {
            [Obsolete("Use Current instead")]
            [InspectorName("<color=orange><label/></color>")]
            Legacy = 1,
            [InspectorName("<color=green><label/></color>")]
            Current = 1,
            Other = 2,
        }

        // Remove: Current remains selectable. Disable: only Legacy is disabled.
        // Include: Current and Legacy retain their own names and colors.
        // Older code may hide/disable both aliases or repeat one alias's label.
        [FieldSeparator("Same-value aliases: Current declared first")]
        [EnumToggleButtons] public AliasCurrentFirst aliasCurrentFirstRemove = AliasCurrentFirst.Current;
        [EnumToggleButtons(EObsolete.Disable)] public AliasCurrentFirst aliasCurrentFirstDisable = AliasCurrentFirst.Current;
        [EnumToggleButtons(EObsolete.Include)] public AliasCurrentFirst aliasCurrentFirstInclude = AliasCurrentFirst.Current;

        [FieldSeparator("Same-value aliases: Legacy declared first")]
        [ValueButtons] public AliasLegacyFirst aliasLegacyFirstRemove = AliasLegacyFirst.Current;
        [ValueButtons(EObsolete.Disable)] public AliasLegacyFirst aliasLegacyFirstDisable = AliasLegacyFirst.Current;
        [ValueButtons(EObsolete.Include)] public AliasLegacyFirst aliasLegacyFirstInclude = AliasLegacyFirst.Current;

        // The menu should contain both distinct alias labels, even though their values match.
        [MenuDropdown] public AliasCurrentFirst aliasMenu = AliasCurrentFirst.Current;

        [Serializable, Flags]
        public enum FlagsWithAliases
        {
            None = 0,
            [Obsolete("Use Current instead")]
            [InspectorName("<color=orange><label/></color>")]
            Legacy = 1,
            [InspectorName("<color=green><label/></color>")]
            Current = 1,
            Other = 2,
        }

        // Older EnumFlagsUtil.GetMetaInfo throws on the duplicate dictionary key (1).
        // Inspecting these fields should succeed; only Legacy is hidden/disabled.
        [FieldSeparator("Flags with same-value aliases")]
        [EnumToggleButtons] public FlagsWithAliases aliasFlagsRemove = FlagsWithAliases.Current;
        [EnumToggleButtons(EObsolete.Disable)] public FlagsWithAliases aliasFlagsDisable = FlagsWithAliases.Current;
        [EnumToggleButtons(EObsolete.Include)] public FlagsWithAliases aliasFlagsInclude = FlagsWithAliases.Current;
    }
}
