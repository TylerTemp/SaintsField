using SaintsField.Playa;
using UnityEngine.SceneManagement;

namespace SaintsField.Samples.Scripts
{
    public class SceneReferenceExample : SaintsMonoBehaviour
    {
        public SceneReference sceneRef;

        [ShowInInspector] public SceneReference s => sceneRef;
        [ShowInInspector] public string scenePath => sceneRef;
        [ShowInInspector] public int sceneIndex => sceneRef.index;

        private void Load()
        {
            SceneManager.LoadScene(sceneRef);  // load by scene path (name)
            SceneManager.LoadScene(sceneRef.index);  // load by scene index
        }
    }
}
