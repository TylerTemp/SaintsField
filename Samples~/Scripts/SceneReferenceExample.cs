using SaintsField.Playa;

namespace SaintsField.Samples.Scripts
{
    public class SceneReferenceExample : SaintsMonoBehaviour
    {
        public SceneReference sceneRef;

        [ShowInInspector]
        public SceneReference s => sceneRef;
    }
}
