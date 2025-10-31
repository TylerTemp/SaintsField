namespace SaintsField.Samples.Scripts
{
    public class SceneExample: SaintsMonoBehaviour
    {
        [
            Scene,
            // RichLabel("<icon=star.png /><label />")
            BelowText("<field/>")
        ]
        public int sceneI;
        [Scene,
            BelowText("$" + nameof(sceneS))
        ]
        public string sceneS;

        [Scene(true),
            BelowText("$" + nameof(fullPathScene))
        ]
        public string fullPathScene;

        [ReadOnly]
        [Scene] public string sceneDisabled;
    }
}
