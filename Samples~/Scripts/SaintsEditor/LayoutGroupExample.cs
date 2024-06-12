using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutGroupExample : MonoBehaviour
    {
        public string beforeGroup;

        [LayoutGroup("group", ELayout.Background | ELayout.TitleOut)]
        public string group1;
        public string group2;
        public string group3;

        [LayoutEnd("group")] public string afterGroup;
    }
}
