using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.UIKit
{
    public class LabelLength : MonoBehaviour
    {
        public GameObject itsALongDriveForPeopleWhoHaveNothingToThinkAbout0;

        [RichLabel("It's a long drive for people with nothing to think about")]
        public string itsALongDriveForPeopleWhoHaveNothingToThinkAbout1;

        [Rate(0, 5)][RichLabel("It's a long drive for people with nothing to think about")]
        public int itsALongDriveForPeopleWhoHaveNothingToThinkAbout2;

        [FieldType(typeof(Dummy))][RichLabel("It's a long drive for people with nothing to think about")]
        public GameObject itsALongDriveForPeopleWhoHaveNothingToThinkAbout3;

        [Dropdown(nameof(GetDropdownItems))][RichLabel("It's a long drive for people with nothing to think about")]
        public float itsALongDriveForPeopleWhoHaveNothingToThinkAbout4;

        private DropdownList<float> GetDropdownItems()
        {
            return new DropdownList<float>
            {
                { "1", 1.0f },
                { "2", 2.0f },
                { "3/1", 3.1f },
                { "3/2", 3.2f },
            };
        }

        [AdvancedDropdown(nameof(AdvDropdown))][RichLabel("It's a long drive for people with nothing to think about")]
        public int itsALongDriveForPeopleWhoHaveNothingToThinkAbout5;

        public AdvancedDropdownList<int> AdvDropdown()
        {
            return new AdvancedDropdownList<int>("Days")
            {
                {"First Half/Monday", 1, false, "star.png"},  // enabled, with icon
                {"First Half/Tuesday", 2},

                {"Second Half/Wednesday/Morning", 3, false, "star.png"},
                {"Second Half/Wednesday/Afternoon", 4},
                {"Second Half/Thursday", 5, true, "star.png"},  // disabled, with icon
                "",  // root separator
                {"Friday", 6, true},  // disabled
                "",
                {"Weekend/Saturday", 7, false, "star.png"},
                "Weekend/",  // separator under `Weekend` group
                {"Weekend/Sunday", 8, false, "star.png"},
            };
        }

        [PropRange(0f, 5f, 0.5f)][RichLabel("It's a long drive for people with nothing to think about")]
        public float itsALongDriveForPeopleWhoHaveNothingToThinkAbout6;

        [MinMaxSlider(-1f, 3f, 0.3f)][RichLabel("It's a long drive for people with nothing to think about")]
        public Vector2 itsALongDriveForPeopleWhoHaveNothingToThinkAbout7;

        [Serializable, Flags]
        public enum BitMask
        {
            None = 0,  // this will be replaced for all/none button
            Mask1 = 1,
            Mask2 = 1 << 1,
            Mask3 = 1 << 2,
            Mask4 = 1 << 3,
            Mask5 = 1 << 4,
            // Mask6 = 1 << 5,
        }

        [EnumFlags][RichLabel("It's a long drive for people with nothing to think about")]
        public BitMask itsALongDriveForPeopleWhoHaveNothingToThinkAbout8;

        [ResizableTextArea][RichLabel("It's a long drive for people with nothing to think about")]
        public string itsALongDriveForPeopleWhoHaveNothingToThinkAbout9;

        [AnimatorState][RichLabel("It's a long drive for people with nothing to think about")]
        public AnimatorState itsALongDriveForPeopleWhoHaveNothingToThinkAbout10;

        [AnimatorParam][RichLabel("It's a long drive for people with nothing to think about")]
        public string itsALongDriveForPeopleWhoHaveNothingToThinkAbout11;

        [Layer][RichLabel("It's a long drive for people with nothing to think about")]
        public string itsALongDriveForPeopleWhoHaveNothingToThinkAbout12;

        [Scene][RichLabel("It's a long drive for people with nothing to think about")]
        public string itsALongDriveForPeopleWhoHaveNothingToThinkAbout13;

        [SortingLayer][RichLabel("It's a long drive for people with nothing to think about")]
        public string itsALongDriveForPeopleWhoHaveNothingToThinkAbout14;

        [Tag][RichLabel("It's a long drive for people with nothing to think about")]
        public string itsALongDriveForPeopleWhoHaveNothingToThinkAbout15;

        [InputAxis][RichLabel("It's a long drive for people with nothing to think about")]
        public string itsALongDriveForPeopleWhoHaveNothingToThinkAbout16;

        [LeftToggle][RichLabel("It's a long drive for people with nothing to think about")]
        public bool itsALongDriveForPeopleWhoHaveNothingToThinkAbout17;

        [CurveRange(-1, -1, 1, 1)][RichLabel("It's a long drive for people with nothing to think about")]
        public AnimationCurve itsALongDriveForPeopleWhoHaveNothingToThinkAbout18;

        [ProgressBar(10)][RichLabel("It's a long drive for people with nothing to think about")]
        public int itsALongDriveForPeopleWhoHaveNothingToThinkAbout19;

        [ResourcePath(typeof(Dummy), typeof(BoxCollider))]
        public string itsALongDriveForPeopleWhoHaveNothingToThinkAbout20;

    }
}
