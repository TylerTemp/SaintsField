#if EXT_INSPECTOR_ENABLE_FOR_SCRIPTABLE_OBJECT
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true)]

    public class ExtInspectorForScriptableObject: ExtBaseInspector
    {

    }
}
#endif
