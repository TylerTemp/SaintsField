using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue70 : SaintsMonoBehaviour
    {
        [GetComponentInChildren] public Dummy[] holders;

        [Button]
        private void PrintHolders()
        {
            foreach (Dummy holder in holders)
            {
                Debug.Log(holder);
            }
        }
    }
}
