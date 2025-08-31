using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.MutipleButton
{
    public class MutipleButtonChild : MutipleButtonParent
    {
        [Button]
        protected override void Parent()
        {
            Debug.Log(s);
        }

        [Button]
        private void Child()
        {
            Debug.Log("Child Button!");
        }
    }
}
