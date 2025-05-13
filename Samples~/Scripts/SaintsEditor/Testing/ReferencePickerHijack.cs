using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ReferencePickerHijack : SaintsMonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        public interface IPicker
        {

        }

        [Serializable]
        public class PickerClass : IPicker
        {
            [LayoutStart("Class", ELayout.Horizontal | ELayout.TitleBox)]
            public int id;
            public string name;
        }

        [Serializable]
        public struct PickerStruct : IPicker
        {
            [LayoutStart("Struct", ELayout.Horizontal | ELayout.TitleBox)]
            public int id;
            public string displayName;
        }

        [SerializeReference]
        public IPicker picker;
#endif

        public PickerStruct pickerStruct;
    }
}
