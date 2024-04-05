using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SceneExample: MonoBehaviour
    {
        [Scene, RichLabel("<icon=star.png /><label />")] public int sceneInt;
        [Scene, BelowRichLabel(nameof(sceneStr), true)] public string sceneStr;
    }
}
