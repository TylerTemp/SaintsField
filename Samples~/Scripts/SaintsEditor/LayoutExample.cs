#if SAINTSFIELD_DOTWEEN
using DG.Tweening;
#endif
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutExample: MonoBehaviour
    {
        // public string above;

        [Ordered]
        [Layout("Root", ELayout.Tab | ELayout.TitleOut | ELayout.Foldout | ELayout.Background)]
        // [Layout("Root", ELayout.Title | ELayout.TitleOutstanding | ELayout.Foldout | ELayout.Background)]
        // [Layout("Root", ELayout.Title)]
        // [Layout("Root", ELayout.Title | ELayout.Background)]
        [Layout("Root/V1")]
        public string hv1Item1;

        [Ordered]
        [Layout("Root/V1/buttons", ELayout.Horizontal)]
        [Button("Root/V1 Button1")]
        public void RootV1Button()
        {
            Debug.Log("Root/V1 Button");
        }
        [Ordered]
        [Layout("Root/V1/buttons", ELayout.Horizontal)]
        [Button("Root/V1 Button2")]
        public void RootV1Button2()
        {
            Debug.Log("Root/V1 Button");
        }

        [Ordered]
        [Layout("Root/V1")]
        [ShowInInspector]
        public static Color color1 = Color.red;
#if SAINTSFIELD_DOTWEEN
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
