using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.UIKit
{
    public class LabelLengthSimpleFix : MonoBehaviour
    {
        [FixLabel]
        public string itsALongRideForPeopleWhoHaveNothingToThinkAbout;
        [FixLabel]
        public string aBitLongerThanDefault;
        [FixLabel]
        public string s;
    }
}
