using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class IMGUIDrawerLabelTextNotWorkingFix : SaintsMonoBehaviour
    {
        public Quaternion origin = Quaternion.identity;

        [LabelText("<color=brown>Quaternion<icon=star.png/>")]
        public Quaternion quaternionValue = Quaternion.identity;

        [LayoutStart("H", ELayout.Horizontal)]

        [LabelText("<color=brown>Quaternion<icon=star.png/>")]
        public Quaternion q1 = Quaternion.identity;

        public Quaternion originH = Quaternion.identity;
    }
}
