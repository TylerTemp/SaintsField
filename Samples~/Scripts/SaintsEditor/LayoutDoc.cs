using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutDoc : SaintsMonoBehaviour
    {
        [LayoutStart("Group", ELayout.FoldoutBox)]
        public int i1;
        public int i2;

        [LayoutStart("./Sub", ELayout.FoldoutBox)]
        public string s1;
        public string s2;

        [LayoutEnd(".")]
        public int i3;
        public int i4;

        [LayoutEnd]
        public GameObject out1;
        public GameObject out2;
    }
}
