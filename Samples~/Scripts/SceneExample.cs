using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SceneExample: MonoBehaviour
    {
        [Scene] public int sceneInt;
        [Scene, BelowRichLabel(nameof(sceneStr), true), RichLabel(null)] public string sceneStr;
    }
}
