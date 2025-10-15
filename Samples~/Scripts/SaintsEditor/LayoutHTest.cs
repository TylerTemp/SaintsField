using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutHTest : SaintsMonoBehaviour
    {
        [Serializable]
        public struct Info
        {
            [LayoutStart("root", ELayout.Horizontal)]

            [FieldLabelText(null)] public Gradient gradient;
            [FieldLabelText(null)]
            public int i;
            [FieldLabelText(null)]
            public string thisIsALongDriveForPeopleWithNothingToThinkAbout;
        }

        [LabelText("<color=pink><label/>"), SaintsRow(inline: true)] public Info[] inputs;

    }
}
