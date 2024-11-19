using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue104 : SaintsMonoBehaviour
    {
        [Button]
        public void AnimateUi()
        {
            Debug.Log("Animating UI");
            AnimateUi(() => { });
        }

        public void AnimateUi(Action callback)
        {
            Debug.Log("Animating UI with arguments");
            // ...
        }
    }
}
