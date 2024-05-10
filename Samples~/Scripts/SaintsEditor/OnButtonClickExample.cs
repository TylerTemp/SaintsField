using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class OnButtonClickExample: MonoBehaviour
    {
        [OnButtonClick(null, 1)]
        public void OnButtonClick(int v)
        {
            Debug.Log($"OnButtonClick {v}");
        }
    }
}
