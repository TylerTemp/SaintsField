using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaintsField.Samples.Scripts
{
    public class SceneExample: MonoBehaviour
    {
        [
            Scene,
            // RichLabel("<icon=star.png /><label />")
        ]
        public int sceneI;
        [Scene,
            // BelowRichLabel(nameof(sceneStr), true)
        ]
        public string sceneS;

        [Scene(true),
            // BelowRichLabel(nameof(sceneStr), true)
        ]
        public string fullPathScene;

        [ReadOnly]
        [Scene] public string sceneDisabled;

        // private void Start()
        // {
        //     SceneManager.LoadScene(fullPathScene);
        // }
    }
}
