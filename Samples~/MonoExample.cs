using System.Collections.Generic;
using ExtInspector.Standalone;
using ExtInspector.Utils;
using ExtInspector;
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
using NaughtyAttributes;
#endif
using UnityEngine;
using EColor = ExtInspector.Utils.EColor;

namespace ExtInspectorUnity.Samples
{
    public class MonoExample : MonoBehaviour
    {
        [SerializeField, Scene] private string _scene;
        [GameObjectActive] public GameObject go;
#if EXT_INSPECTOR_WITH_NAUGHTY_ATTRIBUTES
        [field: Label("TestNALabelOverride"), SerializeField] public string Sth { get; private set; }
#endif

        [field: SepTitle("TestSep", EColor.Green), SerializeField] public int SepTitleTest { get; private set; }
        [field: SepTitle(null, EColor.Green), SerializeField] public int SepTitleJustLine { get; private set; }
    }
}
