using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SceneExample: MonoBehaviour
    {
        [Scene,
            // RichLabel("<icon=star.png /><label />")
        ] public int sceneI;
        [Scene,
            // BelowRichLabel(nameof(sceneStr), true)
        ] public string sceneS;

        [ReadOnly]
        [Scene] public string sceneDisabled;
    }
}
