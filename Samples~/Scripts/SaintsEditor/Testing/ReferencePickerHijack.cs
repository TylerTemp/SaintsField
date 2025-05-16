using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ReferencePickerHijack : SaintsMonoBehaviour
    {
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

#if UNITY_2021_3_OR_NEWER
        [SerializeReference]
        public IPicker picker;
#endif

        public PickerStruct pickerStruct;
    }
}
