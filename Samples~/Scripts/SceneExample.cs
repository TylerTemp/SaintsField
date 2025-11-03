using SaintsField.Playa;

namespace SaintsField.Samples.Scripts
{
    public class SceneExample: SaintsMonoBehaviour
    {
        [
            Scene,
            // RichLabel("<icon=star.png /><label />")
            // BelowText("<field/>")
        ]
        public int sceneI;
        [Scene,
            // BelowText("$" + nameof(sceneS))
        ]
        public string sceneS;

        [Scene(true),
            // BelowText("$" + nameof(fullPathScene))
        ]
        public string fullPathScene;

        [ReadOnly]
        [Scene] public string sceneDisabled;

        [ShowInInspector, Scene]
        private string sceneSRaw
        {
            get => sceneS;
            set => sceneS = value;
        }

        [ShowInInspector, Scene]
        private int sceneIRaw
        {
            get => sceneI;
            set => sceneI = value;
        }
    }
}
