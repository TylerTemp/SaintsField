using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetScriptableObjectExample: MonoBehaviour
    {
        [GetScriptableObject] public Scriptable mySo;
        [GetScriptableObject("RawResources/ScriptableIns")] public Scriptable mySoSuffix;
    }
}
