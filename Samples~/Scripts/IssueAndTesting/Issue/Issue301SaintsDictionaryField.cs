using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue301SaintsDictionaryField : SaintsMonoBehaviour
    {

        [Serializable]
        public struct OneField
        {
            public string f1;
        }

        public SaintsDictionary<int, OneField> oneField;
        public SaintsDictionary<int, OneField[]> oneFields;

        [Serializable]
        public struct TwoField
        {
            public string f1;
            public string f2;
        }

        public SaintsDictionary<int, TwoField> twoField;
        public SaintsDictionary<int, TwoField[]> twoFields;

        [Serializable]
        public struct OneFieldAttr
        {
            public string f1;
        }

        [SaintsDictionary("Id", "Count")]
        public SaintsDictionary<int, OneFieldAttr> oneFieldAttr;

        [Serializable]
        public struct OneFieldLabel
        {
            [NoLabel, FieldAboveText]
            public string f1;
        }

        public SaintsDictionary<int, OneFieldLabel> oneFieldLabel;
    }
}
