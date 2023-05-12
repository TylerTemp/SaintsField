using ExtInspector.Standalone;
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
using NaughtyAttributes;
#endif
using UnityEngine;
using EColor = ExtInspector.Standalone.EColor;

namespace ExtInspector.Samples
{
    public class MonoExample : MonoBehaviour
    {
        [SerializeField, Standalone.Scene] private string _scene;
        [GO] public GameObject _go;
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
        [field: Label("TestLabelOverride"), SerializeField] public string Sth { get; private set; }
#endif

        // [field: LabelDec, SerializeField] public string LabelDecTest { get; private set; }
        [Icon("UnityEditor.SceneView"), SerializeField] private string _labelDecTest2;

        [field: SepTitle("TestSep", EColor.Green), SerializeField] public int SepTitleTest { get; private set; }
    }
}
