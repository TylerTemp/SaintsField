using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.UIKit
{
    public class LabelLengthSaintsField : MonoBehaviour
    {
        [UIToolkit]
        public string itsALongRideForPeopleWhoHaveNothingToThinkAbout;
        [UIToolkit]
        public string aBitLongerThanDefault;
        [UIToolkit]
        public string s;

        [RichLabel("<icon=star.png />It's A Long Ride For People Who Have Nothing To Think About")]
        public string richLabel;
    }
}
