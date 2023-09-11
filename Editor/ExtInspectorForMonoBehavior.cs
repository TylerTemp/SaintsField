#if EXT_INSPECTOR_ENABLE_FOR_MONO_BEHAVIOUR
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]

    public class ExtInspectorForMonoBehavior: ExtBaseInspector
    {

    }
}
#endif
