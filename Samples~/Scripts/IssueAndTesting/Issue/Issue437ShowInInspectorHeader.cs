using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue437ShowInInspectorHeader : SaintsMonoBehaviour
    {
        public string baseField;

        [Header("Default Header")]
        public string defaultHeader;

        [Header("ShowInInspector Header")]
        [ShowInInspector] private int _num1;

        [Separator(13)]
        [AboveText("<b>AboveText Header")]
        [ShowInInspector] private int _num2;

        [Space(13)]
        [AboveText("<b>Space Header")]
        [ShowInInspector] private int _num3;

        [Space(13)]
        [AboveText("<b>Space Header")]
        public int num4;
    }
}
