using System;
using System.Diagnostics;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class DictionaryEnumLabel : SaintsMonoBehaviour
    {
        [Serializable, Flags]
        public enum NamedEnums
        {
            [FieldLabelText("--> <color=gray><label />")]
            Tag1  = 1,
            Tag2 = 1 << 1,
            Tag3 = 1 << 2,
        }

        [AdvancedDropdown]
        public NamedEnums dropEnums;

        [EnumToggleButtons]
        public NamedEnums buttonEnums;

        [FlagsDropdown]
        public NamedEnums flagsEnums;

        [TreeDropdown]
        public NamedEnums treeEnums;

        [FlagsTreeDropdown]
        public NamedEnums flagsTreeEnums;

        [Conditional("UNITY_EDITOR")]
        public class GuildLabelAttribute : FieldLabelTextAttribute
        {
            public GuildLabelAttribute(string label) : base($"{label} <color=gray><label/>")
            {
            }
        }

        public enum GuildHintID
        {
            None = 0,
            First = 1,
            [GuildLabel("Num2")]
            Second = 2,
        }

        [FieldDefaultExpand]
        public SaintsDictionary<GuildHintID, GuildHintID> namedEnumDict;

    }
}
