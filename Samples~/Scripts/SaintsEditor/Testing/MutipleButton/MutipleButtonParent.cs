using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.MutipleButton
{
    public class MutipleButtonParent : SaintsMonoBehaviour
    {
        public string s;

        [Button]
        protected virtual void Parent()
        {
            Debug.Log(s);
        }
    }
}
