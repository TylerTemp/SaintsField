using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class TimeSpanExample : SaintsMonoBehaviour
    {
        [TimeSpan]  // Save value in this
        public long dt;
        // Use this in script
        public TimeSpan MyTimeSpan => new TimeSpan(dt);

        [ShowInInspector] private long v => dt;

        [TimeSpan, DefaultExpand]  // let it expand
        public long expanded;

        [ShowInInspector]
        private TimeSpan _showDt;

        [ShowInInspector, TimeSpan]
        private long _showDtLong;

        [LayoutStart("H", ELayout.Horizontal | ELayout.TitleBox)]
        [TimeSpan]
        public long d1;
        [TimeSpan]
        public long l;
    }
}
