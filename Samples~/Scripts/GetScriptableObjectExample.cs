using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetScriptableObjectExample: MonoBehaviour
    {
        [GetScriptableObject] public Scriptable so;
        [GetScriptableObject("RawResources/ScriptableIns")] public Scriptable soSuffix;
    }
}
