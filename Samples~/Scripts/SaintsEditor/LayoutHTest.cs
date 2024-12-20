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

            [RichLabel(null)] public Gradient gradient;
            [RichLabel(null)]
            public int i;
            [RichLabel(null)]
            public string thisIsALongDriveForPeopleWithNothingToThinkAbout;
        }

        [PlayaRichLabel("<color=pink><label/>"), SaintsRow(inline: true)] public Info[] inputs;

    }
}
