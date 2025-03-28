using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class OverloadButton : SaintsMonoBehaviour
    {
        [Button]
        protected virtual void Override(string s) => Debug.Log(s);

        private void Override(int i) => Debug.Log(i);

        [Button]
        protected virtual void Override(char c) => Debug.Log(c);
    }
}
