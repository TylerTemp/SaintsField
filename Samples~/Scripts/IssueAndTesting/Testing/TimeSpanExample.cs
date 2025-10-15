using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class TimeSpanExample : SaintsMonoBehaviour
    {
        [TimeSpan]  // Save value in this
        public long dt;
        // Use this in script
        public TimeSpan MyTimeSpan => new TimeSpan(dt);

        [ShowInInspector] private long v => dt;

        [Space]
        [TimeSpan, DefaultExpand]  // default use expand mode
        public long expanded;

        [ShowInInspector]
        private TimeSpan _showTs;

        [ShowInInspector, TimeSpan]
        private long _showTsLong;

        [LayoutStart("H", ELayout.Horizontal | ELayout.TitleBox)]
        [TimeSpan]
        public long d1;
        [TimeSpan]
        public long l;
    }
}
