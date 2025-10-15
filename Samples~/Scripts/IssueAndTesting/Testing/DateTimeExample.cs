using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class DateTimeExample: SaintsMonoBehaviour
    {
        [DateTime]  // Save value in this
        public long dt;
        // Use this in script
        public DateTime MyDateTime => new DateTime(dt);

        [ShowInInspector] private long v => dt;

        [ShowInInspector]
        private DateTime _showDt;

        [ShowInInspector, DateTime]
        private long _showDtLong;

        [LayoutStart("H", ELayout.Horizontal | ELayout.TitleBox)]
        [DateTime]
        public long d1;
        [DateTime]
        public long l;
    }
}
