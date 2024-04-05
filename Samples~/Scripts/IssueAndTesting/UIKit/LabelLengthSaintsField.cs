using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.UIKit
{
    public class LabelLengthSaintsField : MonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        [UIToolkit]
        public string itsALongRideForPeopleWhoHaveNothingToThinkAbout;
        [UIToolkit]
        public string aBitLongerThanDefault;
        [UIToolkit]
        public string s;

        [RichLabel("<icon=star.png />It's A Long Ride For People Who Have Nothing To Think About")]
        public string richLabel;

        // lets mix it!
        [Space]

        // default field with UI Toolkit, long
        public string thereIsSomeGoodNewsForPeopleWhoLoveBadNews;
        // UI Toolkit component! Long
        [UIToolkit] public string weWereDeadBeforeTheShipEvenSank;
        // another default, short
        public string myString;
        // another UI Toolkit component! Short
        [UIToolkit] public string myUiToolkit;
#endif
    }
}
