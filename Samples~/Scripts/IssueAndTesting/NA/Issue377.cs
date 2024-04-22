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
            [Dropdown("titles"), BelowRichLabel(nameof(value), true)]
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
            string name = "";
#pragma warning restore 0169
            public TimelineItem()
            {

                name = "TImeline Elemenet";
            }
            public bool label;
            [ShowIf("label")]
            public string LabelText = "Round 1";
            [SaintsRow]
            public List<VideoItem> videos = new List<VideoItem>();
            public bool loop;
            [ShowIf("ShouldShowLoop")]
            public int loopCount=1;
            public bool startTimer;
            [ShowIf("startTimer")]
            public float timerTime = 60;
            bool ShouldShowLoop()
            {
                return loop&&!label;
            }

        }

        [SerializeField, SaintsRow] List<TimelineItem> Timeline = new List<TimelineItem>();
    }
}
