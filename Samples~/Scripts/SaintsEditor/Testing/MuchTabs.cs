using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class MuchTabs : SaintsMonoBehaviour
    {
        [LayoutStart("Tabs", ELayout.Tab)]
        [LayoutStart("./Tab1")]
        public string t1;
        [LayoutStart("../Tab2")]
        public string t2;
        [LayoutStart("../Tab3")]
        public string t3;
        [LayoutStart("../Tab4")]
        public string t4;
        [LayoutStart("../Tab5")]
        public string t5;
        [LayoutStart("../Tab6")]
        public string t6;
        [LayoutStart("../Tab7")]
        public string t7;
        [LayoutStart("../Tab8")]
        public string t8;
        [LayoutStart("../Tab9")]
        public string t9;
        [LayoutStart("../Tab10")]
        public string t10;
    }
}
