using ExtInspector.Standalone;
using ExtInspector.Utils;
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
using NaughtyAttributes;
#endif
using UnityEngine;
using EColor = ExtInspector.Utils.EColor;

namespace ExtInspector.Samples
{
    public class MonoExample : MonoBehaviour
    {
        [SerializeField, Standalone.Scene] private string _scene;
        [GO] public GameObject _go;
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
        [field: Label("TestLabelOverride"), SerializeField] public string Sth { get; private set; }
#endif

        [LabelText("OK", icon: Icon.FA.CHESS_BISHOP, iconColor: EColor.Pink), SerializeField] private string _labelDecTest2;
        [LabelText("OK", EColor.Blue, icon: Icon.FA.EYE_SLASH), SerializeField] private string _labelDecTest3;
        [LabelText(null), SerializeField] private string _labelDecTest4;

        [field: SepTitle("TestSep", EColor.Green), SerializeField] public int SepTitleTest { get; private set; }

        // [field: LabelDec, SerializeField] public string LabelDecTest { get; private set; }
        [LabelSuffix("TestLabelSuffixAttribute", EColor.Blue)]
        public int labelSuffixAttribute;
    }
}
