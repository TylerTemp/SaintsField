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

            [FieldRichLabel(null)] public Gradient gradient;
            [FieldRichLabel(null)]
            public int i;
            [FieldRichLabel(null)]
            public string thisIsALongDriveForPeopleWithNothingToThinkAbout;
        }

        [LabelText("<color=pink><label/>"), SaintsRow(inline: true)] public Info[] inputs;

    }
}
