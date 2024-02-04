using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutExample: MonoBehaviour
    {
        // public string above;

        [Layout("Root", ELayout.Tab | ELayout.Title | ELayout.TitleOutstanding | ELayout.Foldout | ELayout.Background)]
        // [Layout("Root", ELayout.Title)]
        // [Layout("Root", ELayout.Title | ELayout.Background)]
        [Layout("Root/V1")]
        public string hv1Item1;

        [Layout("Root/V1")]
        public string hv1Item2;

        // public string below;

        [Layout("Root/V2")]
        public string hv2Item1;

        [Layout("Root/V2/H", ELayout.Horizontal), RichLabel(null)]
        public string hv2Item2, hv2Item3;
        [Layout("Root/V2")]
        public string hv2Item4;

        [Layout("Root/V3", ELayout.Horizontal)]
        [ResizableTextArea]
        public string hv3Item1, hv3Item2;



        // [Layout("Root", ELayout.Horizontal)]
        // // [TextArea]
        // public string hv3Item1, hv3Item2, hv3Item3;

        // group 2

        // [Layout("H2", ELayout.Horizontal)]
        // [Layout("H2/V1", ELayout.Vertical)]
        // public string h2v1Item1;
        //
        // [Layout("H2/V1")]
        // public string h2v1Item2;
        //
        // [Layout("H2/V2", ELayout.Vertical)]
        // public string h2v2Item1, h2v2Item2;
    }
}
