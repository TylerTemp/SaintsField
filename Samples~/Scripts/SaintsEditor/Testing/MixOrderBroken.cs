using System.Collections;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class MixOrderBroken : SaintsMonoBehaviour
    {
        public string targetDirectory = "Assets/ART";  // 指定目录
        public int newMaxSize = 128;

        public static void ShowWindow()
        {
        }

        [Button("ApplyAndroid")]
        private void ApplyMaxSizeToAndroidTextures()
        {
        }

        [ProgressBar(0, maxCallback: nameof(_maxCount)), NoLabel, FieldReadOnly]
        public int progressBar;

        private int _maxCount = 100;


        [Button("ApplyIOS")]
        private IEnumerator ApplyMaxSizeToAndroidTexturesIOS()
        {
            yield break;
        }
    }
}
