using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class HorizontalLayout : SaintsMonoBehaviour
    {
        [LayoutStart("Horizontal", ELayout.Horizontal)]

        [LayoutStart("./Left Hand", ELayout.TitleBox)]
        public string g11;

        [LayoutStart("../Right Hand", ELayout.TitleBox)]
        public string r1;

        public bool r2;

    }
}
