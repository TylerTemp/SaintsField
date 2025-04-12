using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue200
{
    public class Issue200Se: MonoBehaviour
    {
        // public RectOffset ro2;

        [Serializable]
        public class Sub1
        {
            [ResizableTextArea]
            public string s1;
        }

        [Serializable]
        public class MainC
        {
            public string mainS;
            public Sub1 sub1;
        }

        [BelowInfoBox("$" + nameof(D))] public string debugHolder;

        // [SaintsRow]
        public MainC mainC;

        public RectOffset roUnity;
        [SaintsRow]
        public RectOffset ro2SaintsFieldDraw;
        [ShowIf(true)]
        public RectOffset ro2SaintsFallback;

        private string D()
        {
            return $"mainC is null? {mainC == null}\n" +
            $"ro2 is null? {roUnity == null}\n" +
            $"ro2S is null? {ro2SaintsFieldDraw == null}\n" +
            $"ro2F is null? {ro2SaintsFallback == null}";
        }

    }
}
