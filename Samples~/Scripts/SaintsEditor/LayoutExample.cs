#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using DG.Tweening;
#endif
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutExample: SaintsMonoBehaviour
    {
        public string note;

        [Layout("Titled", ELayout.Title | ELayout.TitleOut)]
        public string titledItem1, titledItem2;

        // title
        [Layout("Titled Box", ELayout.Background | ELayout.TitleOut)]
        public string titledBoxItem1;
        [Layout("Titled Box")]  // you can omit config when you already declared one somewhere (no need to be the first one)
        public string titledBoxItem2;

        // foldout
        [LayoutStart("Collapse", ELayout.CollapseBox)]
        public string collapseItem1;
        public string collapseItem2;

        [LayoutStart("Foldout", ELayout.FoldoutBox)]
        public string foldoutItem1;
        public string foldoutItem2;

        // tabs
        [Layout("Tabs", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Tab1")]
        public string tab1Item1;
        public int tab1Item2;

        [LayoutStart("../Tab2")]
        public string tab2Item1;
        public int tab2Item2;

        [LayoutStart("../Tab3")]
        public string tab3Item1;
        public int tab3Item2;

        // nested groups
        [LayoutStart("Nested", ELayout.Background | ELayout.TitleOut)]
        public int nestedOne;

        [LayoutStart("./Nested Group 1", ELayout.TitleOut)]
        public int nestedTwo;
        public int nestedThree;

        [LayoutStart("./Nested Group 2", ELayout.TitleOut)]
        public int nestedFour;
        public string nestedFive;

        // Unlabeled Box
        [Layout("Unlabeled Box", ELayout.Background)]
        public int unlabeledBoxItem1, unlabeledBoxItem2;

        // Foldout In A Box
        [Layout("Foldout In A Box", ELayout.Foldout | ELayout.Background | ELayout.TitleOut)]
        public int foldoutInABoxItem1, foldoutInABoxItem2;

        // Complex example. Button and ShowInInspector works too
        [Ordered]
        [Layout("Root", ELayout.Tab | ELayout.Foldout | ELayout.Background)]
        // [Layout("Root", ELayout.Title | ELayout.TitleOutstanding | ELayout.Foldout | ELayout.Background)]
        // [Layout("Root", ELayout.Title)]
        // [Layout("Root", ELayout.Title | ELayout.Background)]
        [Layout("Root/V1")]
        [SepTitle("Basic", EColor.Pink)]
        public string hv1Item1;

        [Ordered]
        [Layout("Root/V1/buttons", ELayout.Horizontal)]
        [Button("Root/V1 Button1")]
        public void RootV1Button()
        {
            Debug.Log("Root/V1 Button");
        }
        [Ordered]
        [Layout("Root/V1/buttons")]
        [Button("Root/V1 Button2")]
        public void RootV1Button2()
        {
            Debug.Log("Root/V1 Button");
        }

        [Ordered]
        [Layout("Root/V1")]
        [ShowInInspector]
        public static Color color1 = Color.red;
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
        [Ordered]
        [DOTweenPlay("Tween1", "Root/V1")]
        public Tween RootV1Tween1()
        {
            return DOTween.Sequence();
        }

        [Ordered]
        [DOTweenPlay("Tween2", "Root/V1")]
        public Tween RootV1Tween2()
        {
            return DOTween.Sequence();
        }
#endif

        [Ordered]
        [Layout("Root/V1")]
        public string hv1Item2;

        // public string below;

        [Ordered]
        [Layout("Root/V2")]
        public string hv2Item1;

        [Ordered]
        [Layout("Root/V2/H", ELayout.Horizontal), RichLabel(null)]
        public string hv2Item2, hv2Item3;

        [Ordered]
        [Layout("Root/V2")]
        public string hv2Item4;

        [Ordered]
        [Layout("Root/V3", ELayout.Horizontal)]
        [ResizableTextArea, RichLabel(null)]
        public string hv3Item1, hv3Item2;

        [Ordered]
        [Layout("Root/Buggy")]
        [InfoBox("Sadly, Horizontal is buggy either in UI Toolkit or IMGUI")]
        public string buggy = "See below:";

        [Ordered]
        [Layout("Root/Buggy/H", ELayout.Horizontal)]
        public string buggy1, buggy2, buggy3;

        [Ordered]
        [Layout("Title+Tab", ELayout.Tab | ELayout.TitleBox)]
        [Layout("Title+Tab/g1")]
        public string titleTabG11, titleTabG21;

        [Ordered]
        [Layout("Title+Tab/g2")]
        public string titleTabG12, titleTabG22;

        [Ordered]
        [Layout("All Together", ELayout.Tab | ELayout.Foldout | ELayout.Title | ELayout.TitleOut | ELayout.Background)]
        [Layout("All Together/g1")]
        public string allTogetherG11, allTogetherG21;

        [Ordered]
        [Layout("All Together/g2")]
        public string allTogetherG12, allTogetherG22;
    }
}
