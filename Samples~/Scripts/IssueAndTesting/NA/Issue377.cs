using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue377 : MonoBehaviour
    {
        [Serializable]
        public class VideoItem
        {
            // DeEditorUtils.List titles = new();
            [MenuDropdown("titles"), FieldBelowText(nameof(value), true)]
            public string value;

            public DropdownList<string> titles = new DropdownList<string>
            {
                { "Example1", "Example1" },
                { "Example2", "Example2" },
                { "Example3", "Example3" },
            };

            public VideoItem() {
                // titles = TimelineCreator.sectionTitles;
            }
        }

        // [SaintsRow] public VideoItem[] videoItem;

        [Serializable]
        public class TimelineItem
        {

#pragma warning disable 0169
#pragma warning disable CS0414 // Field is assigned but its value is never used
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once MemberInitializerValueIgnored
            private string name = "";
#pragma warning restore CS0414 // Field is assigned but its value is never used
#pragma warning restore 0169
            public TimelineItem()
            {

                name = "TImeline Elemenet";
            }
            public bool label;
            [FieldShowIf("label")]
            // ReSharper disable once InconsistentNaming
            public string LabelText = "Round 1";
            [SaintsRow]
            public List<VideoItem> videos = new List<VideoItem>();
            public bool loop;
            [FieldShowIf("ShouldShowLoop")]
            public int loopCount=1;
            public bool startTimer;
            [FieldShowIf("startTimer")]
            public float timerTime = 60;
            bool ShouldShowLoop()
            {
                return loop&&!label;
            }

        }

        [SerializeField, SaintsRow] List<TimelineItem> Timeline = new List<TimelineItem>();
    }
}
