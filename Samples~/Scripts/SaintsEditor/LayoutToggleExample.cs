using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutToggleExample : SaintsMonoBehaviour
    {
        public bool editableMain;
        public bool visibleMain;

        [LayoutDisableIf(nameof(editableMain))]
        [LayoutShowIf(nameof(visibleMain))]
        [LayoutStart("Main", ELayout.FoldoutBox)]
        public bool editable1;
        public bool visible1;

        [LayoutDisableIf(nameof(editable1))]
        [LayoutShowIf(nameof(visible1))]
        [LayoutStart("./1", ELayout.FoldoutBox, marginBottom: 10)]
        public int int1;
        public string string1;

        [LayoutStart("..", ELayout.FoldoutBox)]
        public bool editable2;
        public bool visible2;


        [LayoutDisableIf(nameof(editable2))]
        [LayoutShowIf(nameof(visible2))]
        [LayoutStart("./2", ELayout.FoldoutBox)]
        public int int2;
        public string string2;

        [LayoutEnd]
        public string layoutEnd;

        // [EnableIf(EMode.Edit)] public string enableIfEdit;
        // [EnableIf(EMode.Play)] public string enableIfPlay;
        // [EnableIf] public string enableIf;
    }
}
