using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ProgressBar : ProgressBarBase
    {
        [ProgressBar(nameof(minValue)
                , nameof(maxValue)
                , step: 0.05f
                , backgroundColorCallback: nameof(BackgroundColor)
                , colorCallback: nameof(FillColor)
                , titleCallback: nameof(Title)
            ),
        ]
        [RichLabel(null)]
        public float fValue;
    }
}
